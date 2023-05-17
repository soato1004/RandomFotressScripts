using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using DG.Tweening;
using RandomFortress.Common;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Game.Skill;
using RandomFortress.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace RandomFortress.Manager
{
    enum SceneType { Intro, Lobby, Game };
    
    public class GameManager : Singleton<GameManager>
    {
        [Header("게임상태")] 
        public GameMain game;
        public bool isGameOver = false;
        public int CurrentStage = 0;
        public float GameTime = 0;
        public Canvas canvasUI;
        public bool canInteract = false;
        public int TowerCost = 100;
        public bool isSkipStage = false;
        
        [Header("타워")] 
        public int towerCount = 0;
        public const int TOTAL_TOWERS = 14;
        public TowerBase[] Towers = new TowerBase[14];
        public Transform[] TowerFixPos = new Transform[14];
        public bool isPlayGame = false;
        public bool isFocus = false; 
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우
        public int OtherDamage { get; private set; } // 타워 판매시에 그 타워의 데미지 누적합산

        [Header("플레이어")] 
        public BaseSkill[] skillArr;
        public int GameMoney { get; private set; }
        public int PlayerHp { get; private set; }
        
        [Header("몬스터")]
        public List<MonsterBase> monsterList;


        private const int SKILL_SLOT = 3;
        
        //--------------------------------------------------------------------------------------------------------------
        
        // public Transform hpBarParent { get; private set; }
        // public Transform bulletParent { get; private set; }
        // public Transform monsterParent { get; private set; }
        // public Transform towerParent { get; private set; }
        // public Transform towerPosParent { get; private set; }
        // public Transform effectParent { get; private set; }
        
        // public TileInfo[] playerTileInfos;
        // public TileInfo[] otherTileInfos;
        
        // TODO: 타일에 State값을 두고 자동으로 생성되게 수정
        public Vector3Int[] wayPoints;
        private Random random;
        
        //--------------------------------------------------------------------------------------------------------------

        public override void Reset()
        {
            JTDebug.LogColor("GameManager Reset");
            
            monsterList = new List<MonsterBase>();
            random = new Random();
        }

        public override void Terminate() 
        {
            JTDebug.LogColor("GameManager Terminate");
        }

        public void SetupGame()
        {
            // 게임 데이터 초기화
            GameMoney = 1000;
            PlayerHp = 10;
            
            TowerFixPos = game.towerPosParent.GetChildren();
            
            
            // 플레이어 설정

            // 스킬 세팅
            skillArr = new BaseSkill[SKILL_SLOT];

            int skillIndex = DataManager.Instance.playerData.mainSkill;
            SkillCreator(0, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.subSkill_1;
            SkillCreator(1, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.subSkill_2;
            SkillCreator(2, skillIndex);
            
            // TODO: 비율 맞추기
            if (canvasUI != null && canvasUI.transform.localScale != transform.localScale)
            {
                transform.localScale = canvasUI.transform.localScale;
            }
        }

        
        #region Skill

        private void SkillCreator(int slotIndex, int skillIndex)
        {
            GameObject go = new GameObject();
            go.transform.parent = game.skillParent;

            SkillData data;
            
            switch ((Skill)skillIndex)
            {
                case Skill.WATER_SLICE:
                    data =  DataManager.Instance.SkillDataDic[skillIndex];
                    WaterSliceSkill skill_water = go.AddComponent<WaterSliceSkill>();
                    skillArr[slotIndex] = skill_water;
                    break;
                
                case Skill.CHANGE_SLICE:
                    ChangePlaceSkill skill_change = go.AddComponent<ChangePlaceSkill>();
                    skillArr[slotIndex] = skill_change;
                    break;
                
                case Skill.GOOD_SELL:
                    GoodSellSKill skill_good = go.AddComponent<GoodSellSKill>();
                    skillArr[slotIndex] = skill_good;
                    break;

                default:
                    Destroy(go);
                    JTDebug.LogError("Skill Index Not Found");
                    break;
            }
        }
        
        public bool CheckUserAvailableSkill(TowerBase tower, BaseSkill.SkillAction skillAction)
        {
            foreach (BaseSkill skill in skillArr)
            {
                // 해당 상황에 맞는 스킬이 있는지
                foreach (BaseSkill.SkillAction action in skill._skillActions)
                {
                    if (skillAction == action && skill.CanUseSkill())
                    {
                        skill.UseSkill(tower);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public int Range(int min, int max)
        {
            return min + (random.Next()%max);
        }

        public void KillMonster(int reward)
        {
            GameMoney += reward;
            GameUIManager.Instance.UpdateInfo();
        }

        public void DamageToPlayer(int value = 1)
        {
            PlayerHp -= value;
            GameUIManager.Instance.UpdateInfo();
            if (PlayerHp == 0)
            {
                GameOver();
            }
        }

        public bool CanBuildTower()
        {
            if (TowerCost > GameMoney)
                return false;

            GameMoney -= TowerCost;
            return true;
        }

        public void BuildRandomTower()
        {
            if (towerCount == TOTAL_TOWERS)
            {
                // Debug.Log("최대타워 겟수도달");
                return;
            }

            if (!CanBuildTower())
            {
                // Debug.Log("돈부족");
                return;   
            }

            // 타워 위치 랜덤선택
            int posIndex;
            do
            {
                posIndex = Range(0, TOTAL_TOWERS);
            } while (Towers[posIndex] != null);

            // 타워 랜덤 선택
            int towerIndex = GameManager.Instance.Range(0, 8);
            
            BuildTower(posIndex, towerIndex);
        }
        
        public void BuildTower(int posIndex, int towerIndex)
        {
            // 
            TowerFixPos[posIndex].gameObject.SetActive(false);
            
            // 타워 생성
            GameObject towerGo = ObjectPoolManager.Instance.GetTower();
            towerGo.transform.position = TowerFixPos[posIndex].transform.position;
            TowerBase tower = towerGo.GetComponent<TowerBase>();
            tower.Init(posIndex, towerIndex);
            Towers[posIndex] = tower;
            ++towerCount;
            GameUIManager.Instance.UpdateInfo();
            
            SoundManager.Instance.PlayOneShot("DM-CGS-22");
        }

        /// <summary>
        /// 타워 판매
        /// </summary>
        /// <param name="useSkill"> 스킬 사용시 100% 판매금액 </param>
        public void SellTower(bool useSkill = false)
        {
            OtherDamage += focusTower.TotalDamege;
            GameMoney += useSkill ? focusTower.Price :focusTower.SalePrice;
            ResetTowerPos(focusTower);
            isFocus = false;
            focusTower = null;
            GameUIManager.Instance.UpdateInfo();
        }

        public void ResetTowerPos(TowerBase target)
        {
            Towers[target.TowerPosIndex] = null;
            TowerFixPos[target.TowerPosIndex].gameObject.SetActive(true);
            target.RemoveTower();
            --towerCount;
        }

        public Vector3 GetNext(int index)
        {
            return game.tilemap.GetCellCenterWorld(wayPoints[index]);
        }

        public void SkipWaitTime()
        {
            isSkipStage = true;
        }

        public void SkipReward(int reward)
        {
            GameMoney += reward;
        }

        public void GameOver()
        {
            isGameOver = true;
            PauseAllCoroutines();
            GameUIManager.Instance.ShowResult();
        }

        public void ContinueGame()
        {
            isGameOver = false;
        }

        #region Coroutin
        
        private bool _isPaused = false;

        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; }
        }
        
        public void PauseAllCoroutines()
        {
            StopAllCoroutines();
            _isPaused = true;
        }

        public void ResumeAllCoroutines()
        {
            ResumeAllCoroutines();
            _isPaused = false;
        }

        #endregion

    }
}
