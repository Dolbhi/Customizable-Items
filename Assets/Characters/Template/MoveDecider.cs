using UnityEngine;
using System.Collections;
using System;

namespace ColbyDoan
{
    using Physics;

    public class MoveDecider : MonoBehaviour
    {
        public event Action<(Vector2, float)> OnEvaluationFinish;

        public IDirectionEvaluator DirectionEvaluator
        {
            get => _evaluator;
            set
            {
                _evaluator = value;
                if (_evaluator == null)
                    enabled = false;
                else
                    enabled = true;
            }
        }
        IDirectionEvaluator _evaluator;

        [SerializeField] int directionNumber = 16;
        public float updatePeriod = .2f;

        public float maxObstaclesDistance = 2f;
        public float minObstaclesDistance = 1f;

        public float maxCrowdAvoidDistance = 2f;
        public float minCrowdAvoidDistance = 1f;

        public bool requireFloor = true;
        public bool allowDrops = false;

        MoveOption[] _moveOptions;
        (Vector2, float) _bestOption;

        WaitForSeconds _updateWait;
        void Awake()
        {
            _updateWait = new WaitForSeconds(updatePeriod);
            if (_moveOptions == null) PopulateDirections();
        }
        public void PopulateDirections()
        {
            _moveOptions = new MoveOption[directionNumber];
            float angleIncrement = 2 * Mathf.PI / directionNumber;
            for (int i = 0; i < directionNumber; i++)
            {
                float currentAngle = i * angleIncrement;
                MoveOption currentOption = _moveOptions[i] = new MoveOption();
                currentOption.goalScore = 1;
                currentOption.direction = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
            }
        }

        IEnumerator MovementUpdateCoroutine()
        {
            while (true)
            {
                OnEvaluationFinish.Invoke(GetBestMoveOption(_evaluator));
                yield return _updateWait;
            }
        }
        void OnEnable()
        {
            StartCoroutine("MovementUpdateCoroutine");
        }
        void OnDisable()
        {
            StopCoroutine("MovementUpdateCoroutine");
        }

        public (Vector2, float) GetBestMoveOption(IDirectionEvaluator evaluator)
        {
            // check if current direction evaluator is set
            if (evaluator == null)
            {
                Debug.LogWarning("No user set set", this);
                return (_moveOptions[0].direction, 1);
            }

            // do pre-evaluation (stuff that does not need to be done for each direction)
            evaluator.PreEvaluation();

            return GetBestMoveOption(evaluator.EvaluateDirection);
        }
        public (Vector2, float) GetBestMoveOption(Func<Vector2, float> evaluator)
        {
            Vector3 pos = transform.position;

            // get crowd
            Collider2D[] crowd = new Collider2D[10];
            Physics2D.OverlapCircleNonAlloc(pos, maxCrowdAvoidDistance, crowd, PhysicsSettings.animates, pos.z - 1, pos.z);

            // reset scores
            foreach (MoveOption option in _moveOptions) option.Reset();

            // score calc loops over each direction
            for (int i = 0; i < directionNumber; i++)
            {
                MoveOption option = _moveOptions[i];

                // goal
                option.goalScore = evaluator(option.direction);

                // physical obstacles
                RaycastHit2D hit = Physics2D.Raycast(pos, option.direction, maxObstaclesDistance, PhysicsSettings.solids, pos.z, pos.z + 1.5f);
                if (hit)
                {
                    // set score if higher
                    float newScore = Mathf.InverseLerp(maxObstaclesDistance, minObstaclesDistance, hit.distance);
                    option.UpdateObstacleScore(newScore);

                    // blur
                    int previous = i == 0 ? _moveOptions.Length - 1 : i - 1;
                    int next = i + 1 == _moveOptions.Length ? 0 : i + 1;
                    _moveOptions[previous].UpdateObstacleScore(newScore / 2);
                    _moveOptions[next].UpdateObstacleScore(newScore / 2);
                }

                // pit detection
                if (requireFloor)
                {
                    float pitDistance;
                    if (allowDrops)
                    {
                        // detects pits
                        pitDistance = TileManager.Instance.PitRaycast(transform.position, option.direction, maxObstaclesDistance, .1f);
                    }
                    else
                    {
                        // detects drops
                        pitDistance = TileManager.Instance.PitRaycast(transform.position, option.direction, maxObstaclesDistance);
                    }
                    option.UpdateObstacleScore(Mathf.InverseLerp(maxObstaclesDistance, minObstaclesDistance, pitDistance));
                }

                // crowd avoidance
                foreach (Collider2D collider in crowd)
                {
                    if (!collider) continue;
                    //Debug.Log("crowd detected: " + collider.name);
                    Vector2 crowdDirection = collider.transform.position - pos;
                    float dist = crowdDirection.magnitude;
                    crowdDirection.Normalize();
                    float newScore = Vector2.Dot(option.direction, crowdDirection) * Mathf.InverseLerp(maxCrowdAvoidDistance, minCrowdAvoidDistance, dist);
                    option.UpdateObstacleScore(newScore);
                }

                //print("hit?: " + (hit.collider != null) + " dist: " + hit.distance + " value: " + option.desirablility);
            }

            // find best
            int bestOptionIndex = 0;
            for (int i = 0; i < directionNumber; i++)
            {
                if (_moveOptions[bestOptionIndex] == null || _moveOptions[i].FinalScore > _moveOptions[bestOptionIndex].FinalScore)
                    bestOptionIndex = i;
            }

            // interpolate best direction from neighbours
            var bestOption = _moveOptions[bestOptionIndex];
            var upperOption = _moveOptions[(bestOptionIndex + 1) % directionNumber];
            var lowerOption = _moveOptions[(bestOptionIndex - 1 + directionNumber) % directionNumber];

            _bestOption = OptionInterpolation(lowerOption, bestOption, upperOption);
            return _bestOption;
        }
        (Vector2, float) OptionInterpolation(MoveOption o0, MoveOption o1, MoveOption o2)
        {
            var forwardDifference = o1.FinalScore - o0.FinalScore;
            var backDifference = o1.FinalScore - o2.FinalScore;
            var diffSum = forwardDifference + backDifference;

            float fraction = (diffSum == 0) ? .5f : forwardDifference / diffSum;

            var direction = Vector2.Lerp(o0.direction, o2.direction, fraction / 2 + .25f);
            var score = o1.FinalScore;
            // Debug.Log($"{fraction}, o0:{o0.direction} o2:{o2.direction} finalDir:{direction}");
            // Debug.Log($"{fraction}, o0:{o0.FinalScore} o1:{o1.FinalScore} o2:{o2.FinalScore}");
            return (direction, score);
        }

        public void EvaluateImmediately()
        {
            OnEvaluationFinish.Invoke(GetBestMoveOption(_evaluator));
        }

        void OnDrawGizmos()
        {
            if (_moveOptions == null) return;
            foreach (MoveOption option in _moveOptions)
            {
                if (option.FinalScore == 0) continue;
                Gizmos.color = option.FinalScore > 0 ? Color.green : Color.red; //Color.Lerp(Color.red, Color.green, 1);
                Gizmos.DrawRay((Vector2)transform.position + option.direction * .5f, option.direction * Mathf.Abs(option.FinalScore));
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawRay((Vector2)transform.position + _bestOption.Item1 * .5f, _bestOption.Item1 * Mathf.Abs(_bestOption.Item2));
        }

        /// <summary> Options scored to maximize movement in targetDirection </summary>
        public static float TargetDirectionScorer(Vector2 inputDirection, Vector2 targetDirection)
        {
            return (Vector2.Dot(targetDirection.normalized, inputDirection) + 1) / 2;
        }

    }

    public class MoveOption
    {
        public float FinalScore => goalScore - obstacleScore;
        public float goalScore;
        public float obstacleScore;

        public Vector2 direction;

        public void UpdateObstacleScore(float competingScore)
        {
            obstacleScore = Mathf.Max(obstacleScore, competingScore);
        }
        public void Reset()
        {
            goalScore = obstacleScore = 0;
        }
    }

    public interface IDirectionEvaluator
    {
        void PreEvaluation();
        float EvaluateDirection(Vector2 input);
    }
}