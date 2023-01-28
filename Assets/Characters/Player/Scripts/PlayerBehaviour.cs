using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using ColbyDoan.Physics;

namespace ColbyDoan
{
    /// <summary> For handling player inputs </summary>
    public class PlayerBehaviour : MonoBehaviour, IAutoDependancy<Character>
    {
        [Header("Dependancies")]
        [SerializeField] InteractablesFinder interacter;
        [SerializeField] MovementManager movementManager;
        [SerializeField] FrictionManager frictionManager;
        public Inventory inventory;

        // public Texture2D[] cursorByHeight;

        [HideInInspector] public Character character;
        public Character Dependancy { set { character = value; } }

        public bool Sneaking => _sneaking;

        // Movement
        public float jumpSpeed;

        public float sneakingMultiplier = .5f;
        public float normalGroundFriction = 60;
        public float sneakingGroundFriction = 120;
        public float sprintingMultiplier = 1.5f;

        // control inputs
        Controls.PlayerActions playerActions;
        Controls.SkillsActions skillsActions;

        void Awake()
        {
            // setup controls
            Controls controls = new Controls();

            skillsActions = controls.Skills;

            // class skills dict
            inputToSkillDict = new Dictionary<InputAction, int>(6);
            inputToSkillDict.Add(skillsActions.Primary, 0);
            inputToSkillDict.Add(skillsActions.Secondary, 1);
            inputToSkillDict.Add(skillsActions.Special, 2);
            inputToSkillDict.Add(skillsActions.Skill1, 3);
            inputToSkillDict.Add(skillsActions.Skill2, 4);
            inputToSkillDict.Add(skillsActions.Skill3, 5);

            // bind controls
            playerActions = controls.Player;

            playerActions.Interact.performed += _ => Interact();
            playerActions.Jump.performed += _ => Jump();

            playerActions.Sneak.started += _ => Sneak();
            playerActions.Sneak.canceled += _ => UnSneak();

            skillsActions.Get().actionTriggered += ReadSkillInput;

            // bind character events
            character.healthManager.OnDeath += Die;
        }
        Dictionary<InputAction, int> inputToSkillDict;
        bool pointerOverGameObject;
        void ReadSkillInput(InputAction.CallbackContext context)
        {
            if (!character.Alive) return;

            int skillIndex;
            if (inputToSkillDict.TryGetValue(context.action, out skillIndex))
            {
                if (skillIndex >= character.skills.skills.Length) return;

                var skill = character.skills.skills[skillIndex];

                if (context.started)
                {
                    // print("pointerOverGO: " + pointerOverGameObject);
                    if (skillIndex == 0 && pointerOverGameObject)
                        return;
                    skill.Activate();
                }
                else if (context.canceled)
                    skill.Cancel();
            }
        }
        void Jump()
        {
            if (character.kinematicObject.controller.collisions.grounded)
            {
                //    character.kinematicObject.ImpulseTo((Vector2)character.kinematicObject.velocity.normalized * leapSpeed);
                character.kinematicObject.velocity += jumpSpeed * Vector3.forward;
            }
        }
        bool _sneaking = false;
        void Sneak()
        {
            if (!character.Alive) return;
            _sneaking = true;
            frictionManager.groundFriction = sneakingGroundFriction;
            movementManager.speedMultiplier = sneakingMultiplier;
        }
        void UnSneak()
        {
            if (!character.Alive) return;
            _sneaking = false;
            frictionManager.groundFriction = normalGroundFriction;
            movementManager.speedMultiplier = 1;
        }

        public void Interact()
        {
            if (character.Alive)
                interacter.ClosestInteractable?.Interact(this);
        }

        List<Collider2D> _ladders = new List<Collider2D>(1);
        // Vector3 _mousePosSneakOffset = new Vector3(0, 1, -1);
        void Update()
        {
            if (!character.Alive) return;
            // move
            movementManager.UpdateMovement(playerActions.Move.ReadValue<Vector2>(), 1);

            // update cursor and get apparent mouse pos
            Vector3 mousePos = GameManager.Instance.mainCamera.ScreenToWorldPoint(playerActions.Point.ReadValue<Vector2>());
            // terrainHeight at true mouse pos
            int terrainHeight = TileManager.Instance.GetTerrainHeight(mousePos + Mathf.Floor(transform.position.z) * Vector3.down);
            mousePos.z = terrainHeight;
            mousePos.z = Mathf.Floor(transform.position.z) - (_sneaking ? 1 : 0);
            // convert to true pos
            mousePos = mousePos.GetUndisplacedPosition();
            // Cursor.SetCursor(cursorByHeight[(int)mousePos.z + 1], 16 * Vector2.one, CursorMode.Auto);

            // set skills
            foreach (Skill skill in character.skills.skills)
                skill.TargetPos = mousePos;
            character.FacingDirection = mousePos - transform.position;

            // var kinematicObject = character.kinematicObject;

            // // check if on ladder
            // int results = kinematicObject.controller.OverlapCollider(LayerMask.GetMask("Ladder"), _ladders);
            // // print("Ladders: " + results + " Pos: " + transform.position + " Velocity: " + kinematicObject.velocity);
            // if (results != 0)
            // {
            //     //climb
            //     // print(playerActions.Jump.phase);
            //     if (playerActions.Jump.phase == InputActionPhase.Performed) // && transform.position.z < .99f)
            //     {
            //         // print("jump started");
            //         // kinematicObject.controller.collisions.grounded = true;

            //         // check if close enough to top
            //         if (1 - transform.position.z < 0.02f)
            //         {
            //             print("TOP!");
            //             if (!kinematicObject.controller.collisions.grounded)
            //             {
            //                 // Vector2 center = _ladders[0].bounds.center;
            //                 // Vector2 center = _ladders[0].transform.GetChild(0).position;
            //                 Vector2 center = transform.position + _ladders[0].transform.up * .1f;
            //                 kinematicObject.AccelerateTo(Vector3.back * 0.05f, isolation: PhysicsSettings.ForceIsolation.Vertical);
            //                 kinematicObject.Teleport(new Vector3(center.x, center.y, 1.02f));
            //             }
            //         }
            //         else
            //         {
            //             kinematicObject.AccelerateTo(Vector3.forward * 2, isolation: PhysicsSettings.ForceIsolation.Vertical);
            //         }
            //     }
            // }

            pointerOverGameObject = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            // print("pointerOverGO: " + pointerOverGameObject);
        }

        void FixedUpdate()
        {
            var kinematicObject = character.kinematicObject;

            // check if on ladder
            int results = kinematicObject.controller.OverlapCollider(LayerMask.GetMask("Ladder"), _ladders);
            // print("Ladders: " + results + " Pos: " + transform.position + " Velocity: " + kinematicObject.velocity);
            if (results != 0)
            {
                //climb
                // print(playerActions.Jump.phase);
                if (playerActions.Jump.phase == InputActionPhase.Performed) // && transform.position.z < .99f)
                {
                    // print("jump started");
                    // kinematicObject.controller.collisions.grounded = true;

                    // check if close enough to top
                    if (1 - transform.position.z < 0.02f)
                    {
                        // print("TOP!");
                        if (!kinematicObject.controller.collisions.grounded)
                        {
                            // Vector2 center = _ladders[0].bounds.center;
                            // Vector2 center = _ladders[0].transform.GetChild(0).position;
                            Vector2 center = transform.position + _ladders[0].transform.up * .1f;
                            kinematicObject.AccelerateTo(Vector3.back * 0.05f, isolation: PhysicsSettings.ForceIsolation.Vertical);
                            kinematicObject.Teleport(new Vector3(center.x, center.y, 1.02f));
                        }
                    }
                    else
                    {
                        kinematicObject.AccelerateTo(Vector3.forward * 2, isolation: PhysicsSettings.ForceIsolation.Vertical);
                    }
                }
            }
        }

        void OnEnable()
        {
            playerActions.Enable();
            skillsActions.Enable();
        }
        void OnDisable()
        {
            playerActions.Disable();
            skillsActions.Disable();
        }

        void Die()
        {
            GameManager.Instance.TriggerGameOver();
            //maxSpeed = 0;
            //finalSpeed = 0;
            movementManager.speedMultiplier = 0;
        }
    }
}