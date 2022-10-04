// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class Debugger : MonoBehaviour
    {
        public Texture panelTexture;
        public Vector3 pos;

        public Vector2 pos1;
        public Vector2 pos2;
        [Range(0, 1)]
        public float t;

        public DebugAction[] DebugActions;

        // public RankedPools pool;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Vector2.one * 20 + 80 * Vector2.up, Vector2.one * 200), panelTexture);

            foreach (DebugAction debugAction in DebugActions)
            {
                if (GUILayout.Button(debugAction.label))
                {
                    debugAction.action.Invoke();
                }
            }

            GUILayout.EndArea();
        }

        [ContextMenu("TEST ENUMS")]
        void TestEnum()
        {
            Debug.Log($"{EffectModifier.Broken} is {(int)EffectModifier.Broken}");
            Debug.Log($"{EffectModifier.Bundle} is {(int)EffectModifier.Bundle}");
            Debug.Log($"{EffectModifier.None} is {(int)EffectModifier.None}");
        }

        [ContextMenu("TEST LERP")]
        void TestLerp()
        {
            var output = Vector2.Lerp(pos1, pos2, t);
            Debug.Log($"{output.x},{output.y}");
        }

        public Character targetChar;
        public void StunChar()
        {
            targetChar.statusEffects.GetStatus<StunSE>("stun").ApplyStatus(3);
        }

        public void TestRaycast()
        {
            if (PhysicsSettings.SolidsLinecast(Vector3.zero, Vector2.one))
            {
                print("yes");
            }
            else
            {
                print("null");
            }
        }

        public void TestGetTilePos()
        {
            Debug.Log(TileManager.Instance.FloorTiles.LocalToCell(Vector3.forward * 2));
        }

        Dictionary<int, int> test = new Dictionary<int, int>();

        public void IncreaseInt()
        {
            if (!test.ContainsKey(1)) test.Add(1, 0);
            test[1] += 1;
            print(test[1]);
        }

        public void PlusEqualsTest()
        {
            int a = 1;
            print(a += 1);
        }
    }

    [System.Serializable]
    public class DebugAction
    {
        public string label;
        public UnityEvent action;
    }
}