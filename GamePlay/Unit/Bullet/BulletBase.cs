

namespace RandomFortress.Game
{
    public abstract class BulletBase : UnitBase
    {
        public int bulletIndex = 0;
        public MonsterBase target = null;
        public int damage = 1;
        
        public virtual void Init(GamePlayer gPlayer, int index, MonsterBase monster, int value)
        {
            player = gPlayer;
            bulletIndex = index;
            target = monster;
            damage = value;
            
            isDestroyed = false;
            
            // 몬스터가 제거될경우 총알의 타겟을 없앤다
            target.OnUnitDestroy += () =>
            {
                target = null;
            };
        }
    }
}