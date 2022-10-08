// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [System.Serializable]
    public class BobAndWeaveTask : TargetMovementTask
    {
        public float skewAngle = 30;
        public int bobChance = 20;
        int _weaveDirection = 1;

        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // randomly change bob direction
            if (Random.Range(0, 100) < bobChance)
                _weaveDirection = -_weaveDirection;
        }

        public override float EvaluateDirection(Vector2 inputDirection)
        {
            Vector2 weavingDir = Quaternion.Euler(0, 0, _weaveDirection * skewAngle) * targetDisplacement;

            float closenessScore = MoveDecider.TargetDirectionScorer(inputDirection, targetDisplacement);
            float bobingScore = MoveDecider.TargetDirectionScorer(inputDirection, weavingDir);
            return bobingScore;
        }
    }
}
