namespace RandomFortress.Game
{
    [System.Serializable]
    public class TowerCard : ItemCard
    {
        public int Index = 0; // 타워 인덱스
        public int CardLV; // 현재 카드 레벨
        public int CardCount; // 현재 보유중인 카드량
    }
}