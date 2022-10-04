using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Custom Rule Fence", menuName = "Custom Assets/Tiles/Create Custom Rule Fence")]
    public class CustomeRuleFence : CustomRuleTiles
    {
        protected override int GetSpriteIndex(int neighbourMask)
        {
            if ((~neighbourMask & 0b01001000) == 0) return 2; // top right
            if ((~neighbourMask & 0b01010000) == 0) return 0; // top left
            if ((~neighbourMask & 0b00010010) == 0) return 6; // bottom left
            if ((~neighbourMask & 0b00001010) == 0) return 8; // bottom right

            if ((~neighbourMask & 0b01000010) == 0) return 3; // top or bottom present (vertical)
            if ((~neighbourMask & 0b00011000) == 0) return 1; // left or right present (horizontal)

            if ((~neighbourMask & 0b00000010) == 0) return 9; // top
            if ((~neighbourMask & 0b00010000) == 0) return 10; // right
            if ((~neighbourMask & 0b00001000) == 0) return 11; // left
            if ((~neighbourMask & 0b01000000) == 0) return 12; // bottom

            return 13;
        }
    }
}
