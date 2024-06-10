
using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/GameData", order = 1)]
    public class GameData : ScriptableObject
    {
        public int playerHP;
        public int gold;

        GameData()
        {
            playerHP = 5;
            gold = 1000;
        }
    }
    
    // main 스킬은 1000 + index
    // sub 스킬은 2000 + index
}