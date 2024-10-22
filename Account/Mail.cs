using System;

namespace RandomFortress
{
    public enum RewardType { Gold, Gem, TowerCard, SkillCard, Stemina }
    
    public class Mail
    {
        private RewardType type; 
        private int amount;
        private string message;
        public DateTime reciveTime; // 메일 받은 시간
    }
}