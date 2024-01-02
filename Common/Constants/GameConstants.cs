namespace RandomFortress.Constants
{
    public static class GameConstants
    {
        public static readonly int TotalStages = 50;
        
        
        public static string[] GameModeStr = { "Solo", "OneOnOne", "BattleRoyal"};

        #region Game

        public static readonly int TowerCost = 100;

        #endregion

        #region Account

        public static readonly int TOWER_DECK_COUNT = 5;
        public static readonly int SKILL_DECK_COUNT = 3;

        #endregion
        
        #region Tower

        public static readonly int SELECT_TOWER_COUNT = 5;
        public static readonly int[] TOWER_WIDTH_LIST = { 80, 100, 120, 120, 120 };
        public static readonly int[] TOWER_HEIGHT_LIST = { 80, 100, 120, 120, 120 };
        // public static readonly int TOWER_TARGET_WIDTH = 120;
        // public static readonly int TOWER_TARGET_HEIGHT = 120;

        #endregion

        #region Monster

        public enum MonsterState {
            idle, walk, hit, attack, die
        }

        #endregion
        
        #region Debuff

        public static readonly string EmptyMaterial = "EmptyMaterial";
        public static readonly string IceDebuffMaterial = "SlowMaterial";

        #endregion

    }
}