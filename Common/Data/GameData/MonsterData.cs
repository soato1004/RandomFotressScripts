using RandomFortress.Constants;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData", order = 1)]
    public class MonsterData : ScriptableObject
    {
        public int index;
        public string unitName;
        public string prefabName;
        public int hp;
        public int moveSpeed;
        public MonsterType monsterType;
        // public int damage;
        // public int reward;
    }
}