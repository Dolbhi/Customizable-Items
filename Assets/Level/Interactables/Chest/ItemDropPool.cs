using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Item Pool", menuName = "Custom Assets/Loot/Create Item Pool")]
    public class ItemDropPool : ScriptableObject
    {
        public List<Item> artifacts;
    }
}
