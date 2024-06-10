

namespace RandomFortress
{
    public class ToxicStiky : StikyBase
    {
        private int poisonDamage;

        public override void Init(GamePlayer gPlayer, params object[] values)
        {
            poisonDamage = (int)values[0];
            Interval = (int)values[1] / 100f;
            
            base.Init(gPlayer);
        }

        protected override void Hit(MonsterBase monster, TextType type = TextType.Damage)
        {
            monster.Hit(poisonDamage, type);
        }
    }
}