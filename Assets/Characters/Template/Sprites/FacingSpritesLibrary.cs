// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "Facing Sprites Library", menuName = "Custom Assets/Create Facing Sprites Library (Delete later?)")]
    public class FacingSpritesLibrary : ScriptableObject
    {
        // public bool halfFacing = false;

        const char backwardsMovePostfix = 'b';
        public const int directionCount = 8;

        public Map<string, Sprite>[] facingLibraries = new Map<string, Sprite>[directionCount];

        public Sprite GetSprite(FacingDirections facing, string label, bool backwardsMove = false)
        {
            // Debug.Log("AND BEGIN", this);
            // Debug.Log(facingLibraries, this);
            string modLabel = label;
            if (backwardsMove) modLabel += backwardsMovePostfix;

            Sprite output;
            var library = GetFacingLibrary(facing);
            // if (library.DictionaryData.Values.Count == 0)
            // {
            //     Debug.Log("If you see this we cant remove this code yet", this);
            //     library.Refresh();
            // }
            if (library.DictionaryData.TryGetValue(modLabel, out output))
            {
                // Debug.Log("Stuff found", this);
                return output;
            }
            else if (backwardsMove && library.DictionaryData.TryGetValue(label, out output))
            {
                // Debug.Log("Stuff found", this);
                return output;
            }
            // Debug.Log("Stuff not found", this);
            return null;
        }
        public void AddSprite(Sprite sprite, FacingDirections facing, string label)
        {
            var library = GetFacingLibrary(facing);
            library.Add(label, sprite);
            // library.Refresh();
        }
        Map<string, Sprite> GetFacingLibrary(FacingDirections facing)
        {
            var library = facingLibraries[((int)facing)];
            return library;
        }
        [ContextMenu("Validate")]
        void OnValidate()
        {
            foreach (var library in facingLibraries)
            {
                library.Refresh();
            }
        }
    }

    public enum FacingDirections { rb, br, bl, lb, lf, fl, fr, rf }
}