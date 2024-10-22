

namespace RandomFortress
{
    [System.Serializable]
    public class MonsterInfo
    {
        public int index;
        public string unitName;
        public int hp;
        public float moveSpeed;
        public MonsterType monsterType;
        // public int damage;
        // public int reward;
        
        public MonsterInfo() { }
        
        public MonsterInfo(MonsterData data)
        {
            SetData(data);
        }

        public void SetData(MonsterData data)
        {
            index = data.index;
            unitName = data.unitName;
            hp = data.hp;
            moveSpeed = data.moveSpeed;
            monsterType = data.monsterType;
            // damage = data.damage;
            // reward = data.reward;
        }

        public void UpgradeTower(MonsterData data)
        {
            SetData(data);
        }
    }
}