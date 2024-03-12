using RandomFortress.Constants;

namespace RandomFortress.Game
{
    public struct DamageInfo
    {
        public int _damage;
        public TextType _type ;
        
        public DamageInfo(int damage, TextType type = TextType.Damage)
        {
            this._damage = damage;
            this._type = type;
        }
    };
    
    public struct CriticalDamageInfo
    {
        public int _criticalChance; // 치명타확률
        public int _criticalDamage; // 치명타피해. 100을 기준으로 1배.
        
        public CriticalDamageInfo(int criticalChance, int criticalDamage)
        {
            this._criticalChance = criticalChance;
            this._criticalDamage = criticalDamage;
        }
    };
}