using System;
using RandomFortress.Constants;
using RandomFortress.Data;
using UnityEngine;

namespace RandomFortress.Game
{
    public class Account : MonoBehaviour
    {
        [SerializeField] private PlayerData data;

        public int TowerDeck(int index) => data.towerDeck[index];
        public int SkillDeck(int index) => data.skillDeck[index];
        
        void Awake()
        {
            // towerDeck = new int[GameConstants.TOWER_DECK_COUNT];
            // skillDeck = new int[GameConstants.SKILL_DECK_COUNT];
            //
            // Array.Fill(towerDeck, 0);
            // Array.Fill(towerDeck, 0);
        }
        
        public int TowerDeckAddOrRemove(int towerIndex)
        {
            // 중복 되는 타워가 있다면 빼준다
            int size = data.towerDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == towerIndex)
                {
                    data.towerDeck[i] = 0;
                    return 0;
                }
            }
            
            // 빈슬롯이 있다면 추가한다
            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == 0)
                {
                    data.towerDeck[i] = towerIndex;
                    return towerIndex;
                }
            }
            
            // 슬롯에 자리가없다면 아무런 행동도 하지않는다
            return 0;
        }
        
        public int SkillDeckAddOrRemove(int skillIndex)
        {
            // 중복 되는 타워가 있다면 빼준다
            int size = data.skillDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == skillIndex)
                {
                    data.skillDeck[i] = 0;
                    return 0;
                }
            }
            
            // 빈슬롯이 있다면 추가한다
            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == 0)
                {
                    data.skillDeck[i] = skillIndex;
                    return skillIndex;
                }
            }
            
            // 슬롯에 자리가없다면 아무런 행동도 하지않는다
            return 0;
        }
    }
}