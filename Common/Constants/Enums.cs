namespace RandomFortress.Data
{
    public enum Skill
    {
        WATER_SLICE = 1001,
        CHANGE_SLICE = 2001,
        GOOD_SELL,
    }

    public enum TowerAttackType
    {
        None,
        Single,
        Multiple,
        Splash,
        Sticky,
    }
    
    public enum GameType
    {
        Solo, OneOnOne, BattleRoyal
    }
    
    public enum TowerStateType {
        Idle, Attack, Upgrade, Sell
    }
}