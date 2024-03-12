namespace RandomFortress.Data
{
    [System.Serializable]
    public class ExtraInfo
    {
        public int atk = 0; // 공격력
        public int atkSpeed = 0; // 공격속도의 x/100배
        public int atkRange = 0; // 어느 거리안에 들어와야 공격하는지
        public int criChance = 0; // 치명타확률. x/100배
        public int criAtk = 0; // 치명타피해. x/100 배
        public int rewardMonster = 0; // 처치시 추가골드 
        public int cooltime = 0; // 스킬 쿨타임 감소
        public int mulAtk = 0; // 다중공격

        public void AddExtraInfo(ExtraInfo info)
        {
            atk += info.atk;
            atkSpeed += info.atkSpeed;
            atkRange += info.atkRange;
            rewardMonster += info.rewardMonster;
            cooltime += info.cooltime;
            mulAtk += info.mulAtk;
        }
    }
}