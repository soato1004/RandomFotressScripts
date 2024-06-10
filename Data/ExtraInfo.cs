namespace RandomFortress
{
    /// <summary>
    /// 기본타워 능력치 이외의 특수능력치. 어빌리티카드는 플레이어에서 관리. 개별타워 특수능력은 개별타워에 포함됨
    /// </summary>
    [System.Serializable]
    public class ExtraInfo
    {
        public int atk = 0; // 공격력
        public int atkSpeed = 0; // 공격속도의 x/100배
        public int atkRange = 0; // 어느 거리안에 들어와야 공격하는지
        public int criChance = 0; // 치명타확률. x/100배
        public int criAtk = 0; // 치명타피해. x/100 배
        public int rewardMonster = 0; // 처치시 추가골드 
        public int cooldownReduction = 0; // 스킬 쿨타임 감소
        public int mulAtk = 0; // 다중공격
        public int atkRadius = 0; // 범위공격의 영역크기
        public int tickTime = 0; // 적용되는 틱 시간

        public int slowDuration = 0;
        public int slowPercent = 0;
        
        public int stunChance = 0; // 스턴 확률
        public int stunDuration = 0; // 스턴 지속시간
        
        public int burnDuration = 0; // 불 지속시간
        public int burnAtk = 0; // 불 데미지
        

        public int stikyDuration = 0;
        public int stikyAtk = 0;
        
        
        
        public void AddExtraInfo(ExtraInfo info)
        {
            atk += info.atk;
            atkSpeed += info.atkSpeed;
            atkRange += info.atkRange;
            rewardMonster += info.rewardMonster;
            cooldownReduction += info.cooldownReduction;
            mulAtk += info.mulAtk;
        }
    }
}