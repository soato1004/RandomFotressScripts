namespace RandomFortress
{
    // 불타워 총알
    public class ShotBullet : BulletBase
    {
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
        }
    }
}