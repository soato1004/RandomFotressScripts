using RandomFortress.Common.Utils;
using RandomFortress.Constants;

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
            poisonDuration = (int)values[1] / 100;

            // DelayCallUtils.DelayCall(poisonDuration, () =>
            // {
            //     Release();
            // });
            // Destroy(gameObject, poisonDuration);
        }

        protected override void Hit(MonsterBase monster, TextType type = TextType.Damage)
        {
            // TODO: 틱 시간 하드코딩
            monster.Hit(poisonDamage, type);
        }
    }
}