using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


namespace ColbyDoan
{
    // single icon to indicate status effect
    public class StatusIndicator : MonoBehaviour
    {
        [SerializeField] UnityEngine.U2D.Animation.SpriteLibraryAsset statusSprites;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text countText;

        public List<IStatusEffect> activeEffects;
        IStatusEffect currentSE;
        public int index;

        StateMachine stateMachine = new StateMachine();
        ActiveState activeState = new ActiveState();
        InactiveState inactiveState = new InactiveState();

        void Start()
        {
            activeState.Init(this, stateMachine);
            inactiveState.Init(this, stateMachine);

            stateMachine.Initialize(activeState);
        }

        void Update()
        {
            stateMachine.CurrentState.Update();
        }

        class ActiveState : State<StatusIndicator>
        {
            public override void Enter()
            {
                base.Enter();

                // set effect
                container.currentSE = container.activeEffects[container.index];
                // enable icon
                container.icon.enabled = true;
                // set icon
                container.icon.sprite = container.statusSprites.GetSprite("Status Effects", container.currentSE.Name);
            }

            public override void Update()
            {
                base.Update();

                // enable text if there are multiple stacks
                container.countText.enabled = container.currentSE.StackCount > 1;
                // set text
                container.countText.text = container.currentSE.StackCount.ToString();

                if (container.index >= container.activeEffects.Count)
                {
                    // no effect
                    sm.ChangeState(container.inactiveState);
                }
                else if (container.currentSE != container.activeEffects[container.index])
                {
                    // effect is different
                    sm.ChangeState(container.activeState);
                }
            }
        }
        class InactiveState : State<StatusIndicator>
        {
            public override void Enter()
            {
                base.Enter();

                // deactivate
                container.currentSE = null;
                container.icon.enabled = false;
                container.countText.enabled = false;
            }

            public override void Update()
            {
                base.Update();
                if (container.index < container.activeEffects.Count)
                {
                    sm.ChangeState(container.activeState);
                }
            }
        }
    }
}
