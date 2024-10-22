

using System;

namespace RandomFortress
{
    public enum AdRewardType
    {
        AbilityCard = 2, // 어빌리티카드 선택지 추가 버프
        Stamina = 3
    }
    
    [Serializable]
    public class AdDebuff
    {
        public AdRewardType type;
        public string endTime;    
    }
}