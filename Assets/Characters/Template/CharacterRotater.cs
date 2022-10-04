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
        public void SetSpriteFacing(FacingDirections toSet)
        {
            facing = toSet;
            UpdateSprite();
        }
        [ContextMenu("Update sprite")]
        void UpdateSprite()
        {
            renderer.sprite = facingSprites.GetSprite(facing, spritelabel);
        }

        void Update()
        {
            animator.SetFloat("Speed", Mathf.Sign(Vector2.Dot(character.Velocity, character.FacingDirection)) * character.Velocity.magnitude);

            float angle = character.FacingAngle;
            angle = angle < 0 ? 360 + angle : angle;

            FacingDirections newFacing = (FacingDirections)Mathf.FloorToInt(angle / 45);
            if (newFacing != facing)
            {

                // if (transform.root.name == "Player")
                //     Debug.Log(angle, this);

                // set facing
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