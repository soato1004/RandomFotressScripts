

using UnityEngine;

namespace RandomFortress
{
    public class ToxicStiky : StikyBase
    {
        private int poisonDamage;
        private TowerBase attacker;

        public override void Init(GamePlayer gPlayer, params object[] values)
        {
            poisonDamage = (int)values[0];
            Interval = (int)values[1] / 1000f;
            attacker = (TowerBase)values[2];

            textType = TextType.DamagePoison;
            
            base.Init(gPlayer);
        }

        protected override void Hit(MonsterBase monster)
        {
            monster.Hit(poisonDamage, textType);
            attacker.AddDamage(poisonDamage);
        }
    }
}