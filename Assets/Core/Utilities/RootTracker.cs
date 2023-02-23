// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    /// <summary>
    /// Places transform root in a key labelled hashset when enabled
    /// </summary>
    public class RootTracker : MonoBehaviour
    {
        static Dictionary<string, HashSet<Transform>> rootSets = new Dictionary<string, HashSet<Transform>>();

        public static HashSet<Transform> GetSet(string key)
        {
            HashSet<Transform> output;
            if (rootSets.TryGetValue(key, out output))
                return output;
            return null;
        }

        [SerializeField] string setKey;
        HashSet<Transform> _ownSet;
        Transform _root;

        public string TrackingKey => setKey;

        void Awake()
        {
            _root = transform.root;
            if (!rootSets.TryGetValue(setKey, out _ownSet))
            {
                _ownSet = new HashSet<Transform>();
                rootSets.Add(setKey, _ownSet);
            }
        }

        void OnEnable()
        {
            _ownSet.Add(_root);
        }
        void OnDisable()
        {
            _ownSet.Remove(_root);
        }
    }
}
