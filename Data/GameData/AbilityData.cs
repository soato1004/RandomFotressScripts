
using UnityEngine;

namespace RandomFortress
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData", order = 1)]
    public class AbilityData : ScriptableObject
    {
        public int index;        
        public string abilityName;
        public string iconName;
        public Rarity rarity;
        public int percent;
        public ExtraInfo extraInfo;
        public string[] explan = { "", "" };
    }
    
    // main 스킬은 1000 + index
    // sub 스킬은 2000 + index
}