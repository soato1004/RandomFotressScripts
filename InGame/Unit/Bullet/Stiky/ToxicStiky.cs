using RandomFortress.Common.Utils;

namespace RandomFortress.Game
{
    public class ToxicStiky : StikyBase
    {
        private int poisonDamage;
        private int poisonDuration;

        public override void Init(GamePlayer gPlayer, params object[] values)
        {
            base.Init(gPlayer);
            
            poisonDamage = (int)values[0];
            poisonDuration = (int)values[1];

            Destroy(gameObject, poisonDuration);
        }

        protected override void Hit(MonsterBase monster)
        {
            monster.Hit(poisonDamage);
        }
    }
}