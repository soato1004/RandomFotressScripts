namespace RandomFortress
{
    public class ShinoTower : TowerBase
    {
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }

        protected override void Shooting()
        {
            base.Shooting();
        }
    }
}