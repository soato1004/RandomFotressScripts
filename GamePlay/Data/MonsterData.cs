using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MonsterData", order = 1)]
    public class MonsterData : ScriptableObject
    {
        public int index;
        public string unitName;
        public int hp;
        public int moveSpeed;
        public int monsterType;
        public int damage;
        public int reward;
        
        public MonsterData() { }

        public MonsterData(MonsterData other)
        {
            index = other.index;
            unitName = other.unitName;
            hp = other.hp;
            moveSpeed = other.moveSpeed;
            monsterType = other.monsterType;
            damage = other.damage;
            reward = other.reward;
        }
    }
}