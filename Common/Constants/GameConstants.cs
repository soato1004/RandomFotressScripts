namespace RandomFortress
{
    public static class GameConstants
    {
        public static readonly int TotalStages = 50;
        
        
        public static string[] GameModeStr = { "Solo", "OneOnOne", "BattleRoyal"};
        
        #region Game

        public static readonly float atkRangeMul = 1.6f; // 솔로모드에서 사정거리는 더 길어져야한다
        
        public static readonly int MonsterCount = 10; // 몬스터의 최대 생성 수
        public static readonly int SpecialMonsterCount = 5; // 특수 몬스터의 최대 생성 수
        public static readonly int AppearDelay = 100; // 몬스터 생성 간격 1초
        public static readonly int StageClearDelay = 300; // 시작 딜레이 3초
        
        public static readonly int MonsterReward = 10; // 몬스터 처치시 획득 골드
        public static readonly int BossReward = 100; // 보스 처치시 획득 골드
        public static readonly int MonsterDamage = 1;
        public static readonly int BossDamage = 10;

        public static readonly float DebuffTickTime = 0.2f;

        public static readonly float MaxTowerSelectionDuration = 2f;

        public static readonly string PrefabNameHpBar = "HpBar";
        public static readonly string TowerSeatImageName = "Seat_";
        
        #endregion

        #region Stage

        public static readonly float SkipDelayTime = 5f;

        #endregion

        #region AbilityCard

        public static readonly int AbilityCardCount = 4; // 화면에 보여지는 선택가능한 카드숫자
        
        // 어빌리티가 등장하는 스테이지
        public static readonly int[] AbilityStage = { 1, 10, 20, 30, 40 };
        
        // 스테이지 고정 카드
        // public static readonly Rarity[][] AbilityChance = { // 첫 스테이지 + 보스클리어 첫 스테이지마다 어빌리티 부여
        //     new Rarity[] { Rarity.Common, Rarity.Common, Rarity.Rare, Rarity.Rare },
        //     new Rarity[] { Rarity.Rare, Rarity.Rare, Rarity.Epic, Rarity.Epic},
        //     new Rarity[] { Rarity.Epic, Rarity.Epic, Rarity.Legend, Rarity.Legend},
        //     new Rarity[] { Rarity.Legend, Rarity.Legend, Rarity.Unique, Rarity.Unique},
        //     new Rarity[] { Rarity.Unique, Rarity.Unique, Rarity.Unique, Rarity.Unique},
        // };

        // 어빌리티의 스테이지레벨에 따른 등장 확률
        public static readonly float[] RarityParcent =
        {
            80, 20, 10, 2, 1
        };
        
        public static readonly float[] LastRarityParcent =
        {
            0, 80, 20, 10, 2
        };

        // 스테이지별 차등 확률
        // public static readonly int[][] RarityParcent =
        // {
        //     new int[] { 800, 200, 100, 20, 10 },
        //     new int[] { 400, 400, 150, 10, 0 },
        //     new int[] { 200, 200, 200, 6, 0 },
        //     new int[] { 0, 100, 25, 8, 4 },
        //     new int[] { 0, 0, 30, 10, 5 },
        // };

        #endregion

        #region Bullet

        public static readonly int BulletMoveSpeed = 1400; // 총알 이동속도

        #endregion
        
        #region Account

        public static readonly int TOWER_DECK_COUNT = 8; // 타워덱 매수
        public static readonly int TOWER_UPGRADE_COUNT = 5; // 먼저 선택한 5개의 타워만 업그레이드된다
        public static readonly int SKILL_DECK_COUNT = 3; // 스킬덱 매수

        #endregion
        
        #region Tower
        
        public static readonly int TowerCost = 100; // 타워를 건설하는데 필요한 금액
        public static readonly int TowerUpgradeCost = 100; // 타워 업그레이드에 필요한 금액        
        #endregion

        #region Monster
        

        #endregion
        
        #region Debuff

        public static readonly int MonsterTypeLimit = 10000; // 몬스터인덱스가 10000보다 크면 특수 몬스터
        
        public static readonly string AllinDefaultMaterial = "AllinDefaultMaterial"; //
        public static readonly string IceDebuffMaterial = "SlowMaterial"; // 슬로우 이펙트
        public static readonly string BurnDebuffMaterial = "BurnMaterial"; // 슬로우 이펙트
        
        
        public static readonly string SpeedMonsterEffPrefab = "Electricity_Square"; // 스피드형 이펙트
        public static readonly string TankMonsterEffPrefab = "Flame_circle_loop"; // 탱커형 이펙트

        #endregion

        #region SKill

        public static readonly float SkillChoiceWaitTime = 5f; // 스킬 사용후 선택해야될때 최대 대기시간

        #endregion

        //TODO: 서버에서 해당작업들을 수행해야함
        #region AdMob

        public static readonly int AdDebuffMinute = 30; // 광고 시청 후 디버프 유지 30분
        #endregion
    }

    public enum AdDebuffType
    {
        AbilityCard = 0, // 어빌리티카드 선택지
    }
    
    public enum MonsterState {
        idle, walk, hit, attack, die, stun
    }
    
    public enum TowerIndex
    {
        Elephant = 1,   // 1
        Drumble = 2,    // 2
        Sting = 3,      // 3
        Flame = 4,      // 5
        Machinegun = 5, // 6
        Shino = 6 ,     // 7
        MaskMan = 7,    // 8
        Swag = 8,       // 9
    }
    
    public enum BulletIndex
    {
        Common = 0, // 기본 철포
        CannonBall = 1,
        DrumRoll = 2,
        Firecracker = 5,
        Shot = 6,
        Shuriken = 7,
        ToxicShot = 8,
        IceShot = 9,
    }
    
    /// <summary>
    /// 디버프의 종류
    /// </summary>
    // public enum DebuffType
    // {
    //     None,
    //     Poison,
    //     Slow,
    //     Ice,
    //     Burn
    // }
    
    public enum DebuffIndex
    {
        None = -1,
        Ice,
        Burn,
        Stun,
    }
    
    
    // 게임맵은 0~10, 게임유닛은 11~20 안에서
    public enum GameLayer
    {
        MonsterEffIn = 11,
        Monster = 12,
        MonsterHP = 13,
        MonsterEffOut = 14,
        
        Tower = 13,
        TowerDrag = 14,
        
        Bullet = 15,
        
        Boss = 16,
        BossHP = 17,
    }

    public enum CanvasLayer
    {
        Dim,        // 10
        Monster,    // 13
        Boss,       // 17
    }
    
    public enum SceneName
    {
        Logo,
        Bootstrap,
        Lobby,
        Match,
        Game,
    };

    public enum TextType
    {
        ShowGold,
        Damage,
        DamageCritical,
        DamageSlow,
        DamagePoison,
        DamageBurn
    }

    public enum GameRank
    {
        Beginner,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        GrandMaster,
        Challenger
    }

    public enum MonsterType
    {
        None,
        Normal,
        Speed = 10000,
        Tank = 20000,
        Boss = 30000,
    }
    
    public enum MonsterIndex
    {
        WolfBlue = 2001,
        WolfGreen,
        WolfRed,
        GoblinBerserkr = 2004,
        GoblinMagician,
        GoblinWarrior, 
        SkeletonArcher = 2007,
        SkeletonWarrior,
        SkeletonWizard,
        Ogre = 2010,
        OgreBerserkr,
        OgreWarrior,
        Golem = 2012,
        GolemBlue,
        GolemIce,
        Archer_16 = 2015,
        Wizard_16,
        Assassin_17
    }

    public enum BossIndex
    {
        Boss1 = 3001,
        Boss2,
        Boss3,
        Boss4,
        Boss5,
    }
    
    public enum GameType
    {
        Solo, OneOnOne, BattleRoyal
    }
    
    public enum TowerStateType {
        Idle, Attack, Upgrade, Sell
    }

    public enum Rarity
    {
        Common, Rare, Epic, Legend, Unique
    }

    public enum SkillIndex
    {
        WaterSlice = 8001,
        ChangePlace = 7001,
        GoodSell = 7002,
    }
    
}