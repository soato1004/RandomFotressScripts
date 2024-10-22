using System.Collections;
using System.Collections.Generic;



using UnityEngine;

namespace RandomFortress
{
    public abstract class BaseSkill : MonoBehaviour
    {
        protected SkillData data;
        protected GamePlayer player;
        public bool useSkill { get; protected set; }
        
        protected SkillButton _skillButton;
        protected bool _canUseSkill = true;
        protected float _coolTime;
        
        public float skillCoolTime => _coolTime; // 스킬의 사용딜레이
        public float skillCoolTimer { get; private set; } // 현재 스킬 대기시간
        public bool CanUseSkill() => _canUseSkill;
        
        // 특정 행동을 취할때 반응하는 스킬을 찾는다
        public enum SkillAction 
        {
            Touch_Tower,
            Drag_Tower,
            // Touch_OtherTower,
        }
        public List<SkillAction> _skillActions = new List<SkillAction>();
        
        // 영웅, 서브 스킬의 구분
        public enum SkillType
        {
            None, 
            Hero, 
            Sub
        }
        public SkillType skillType = SkillType.None;
        public SkillData Data => data;
        
        public abstract void Init(int skillIndex, GamePlayer gPlayer,SkillButton button);

        public abstract void SkillStart();
        
        public abstract void UseSkill(params object[] values);
        
        /// <summary>
        /// 스킬 쿨다운 타이머 시작
        /// </summary>
        /// <param name="coolTime"></param>
        /// <returns></returns>
        protected IEnumerator WaitCoolTimeCor(float coolTime)
        {
            GameManager.I.canTowerDrag = true;
            
            if (_canUseSkill == false || !player.IsLocalPlayer)
                yield break;
            
            _canUseSkill = false;
            skillCoolTimer = 0;
            _skillButton.wait.SetActive(true);
            
            float coolTimer = coolTime * (1f - (player.extraInfo.cooldownReduction / 100f));
            while (skillCoolTimer < coolTimer)
            {
                if (GameManager.I.isGameOver)
                    yield break;
                
                _skillButton.coolTime.text = "" + (int)(coolTime - skillCoolTimer);
                skillCoolTimer += Time.deltaTime * GameManager.I.gameSpeed;
                yield return null;
            }
            
            
            _skillButton.button.interactable = true;
            _skillButton.wait.SetActive(false);
            _canUseSkill = true;
            Debug.Log("Skill Ready " + data.skillName);
        }
        
        // 5초간 입력이 없으면 스킬 사용으로 처리
        public IEnumerator WaitAndForceSkillUseCor()
        {
            float waitTime = 0;
            
            while (waitTime < GameConstants.SkillChoiceWaitTime)
            {
                waitTime += Time.deltaTime * GameManager.I.gameSpeed;
                yield return null;
            }
            
            Debug.Log("스킬사용 후 선택을 안함 "+data.skillName);
            
            useSkill = false;
            GameManager.I.SetSkillDim(false,player);
            StartCoroutine(WaitCoolTimeCor(_coolTime/2));
        }
    }
}