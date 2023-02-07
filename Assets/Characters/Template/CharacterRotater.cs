using UnityEngine;


namespace ColbyDoan
{
    public class CharacterRotater : MonoBehaviour
    {
        [SerializeField] Character character = null;

        [SerializeField] Animator animator = null;
        [SerializeField] new SpriteRenderer renderer = null;

        public FacingSpritesLibrary facingSprites;

        [SerializeField] FacingDirections facing;
        [SerializeField] bool movingBackwards;
        [SerializeField] string spritelabel;

        [SerializeField] bool doSpriteValidation;

        void OnValidate()
        {
            if (doSpriteValidation)
                UpdateSprite();
        }

        public void SetSpriteLabel(string label)
        {
            spritelabel = label;
            UpdateSprite();
        }
        void SetSpriteFacing(FacingDirections toSet)
        {
            facing = toSet;
            UpdateSprite();
        }
        [ContextMenu("Update sprite")]
        void UpdateSprite()
        {
            Sprite fSprite = facingSprites.GetSprite(facing, spritelabel, movingBackwards);
            if (fSprite == null)
            {
                fSprite = facingSprites.GetSprite(facing.Mirror(), spritelabel, movingBackwards);
                renderer.flipX = true;
            }
            else
            {
                renderer.flipX = false;
            }
            renderer.sprite = fSprite;
        }

        void Update()
        {
            int speedVFacing = (int)Mathf.Sign(Vector2.Dot(character.Velocity, character.FacingDirection));
            animator.SetFloat("Speed", speedVFacing * character.Velocity.magnitude);

            float angle = character.FacingAngle;
            angle = angle < 0 ? 360 + angle : angle;

            FacingDirections newFacing = (FacingDirections)Mathf.FloorToInt(angle / 45);
            bool newMovingBackwards = speedVFacing == -1;
            if (newFacing != facing || newMovingBackwards != movingBackwards)
            {

                // if (transform.root.name == "Player")
                //     Debug.Log(angle, this);

                // set facing
                movingBackwards = newMovingBackwards;
                SetSpriteFacing(newFacing);
                // switch (facing)
                // {
                //     case 0:
                //         spriteResolver.SetCategoryAndLabel("rb", spriteResolver.GetLabel());
                //         break;
                //     case 1:
                //         spriteResolver.SetCategoryAndLabel("br", spriteResolver.GetLabel());
                //         break;
                //     case 2:
                //         spriteResolver.SetCategoryAndLabel("bl", spriteResolver.GetLabel());
                //         break;
                //     case 3:
                //         spriteResolver.SetCategoryAndLabel("lb", spriteResolver.GetLabel());
                //         break;
                //     case 4:
                //         spriteResolver.SetCategoryAndLabel("lf", spriteResolver.GetLabel());
                //         break;
                //     case 5:
                //         spriteResolver.SetCategoryAndLabel("fl", spriteResolver.GetLabel());
                //         break;
                //     case 6:
                //         spriteResolver.SetCategoryAndLabel("fr", spriteResolver.GetLabel());
                //         break;
                //     case 7:
                //         spriteResolver.SetCategoryAndLabel("rf", spriteResolver.GetLabel());
                //         break;
                // }
            }
        }
    }

    public interface IFacingCharacter
    {
        Vector2 FacingDirection { get; }
        float FacingAngle { get; }

        Vector2 Velocity { get; }
    }
}