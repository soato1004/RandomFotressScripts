using RandomFortress.Constants;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class ShinoTower : TowerBase
    {
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            base.Init(targetPlayer, posIndex, towerIndex);
        }

        protected override void Shooting()
        {
            base.Shooting();
        }
    }
}