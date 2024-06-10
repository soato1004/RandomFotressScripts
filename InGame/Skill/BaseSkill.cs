using System.Collections;
using System.Collections.Generic;

using RandomFortress.Data;

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
        
        protected IEnumerator WaitCoolTimeCor(float coolTime)
        {
            if (_canUseSkill == false || !player.isLocalPlayer)
                yield break;
            
            _canUseSkill = false;
            skillCoolTimer = 0;
            _skillButton.wait.SetActive(true);
            
            float coolTimer = coolTime * (1f - (player.extraInfo.cooldownReduction / 100f));
            while (skillCoolTimer < coolTimer)
            {
                if (GameManager.Instance.isGameOver)
                    yield break;
                
                _skillButton.coolTime.text = "" + (int)(coolTime - skillCoolTimer);
                skillCoolTimer += Time.deltaTime * GameManager.Instance.TimeScale;
                yield return null;
            }
            
            
            _skillButton.button.interactable = true;
            _skillButton.wait.SetActive(false);
            _canUseSkill = true;
            Debug.Log("Skill Ready!!");
        }
        
        // 5초간 입력이 없으면 스킬 사용으로 처리
        public IEnumerator WaitSkillUseCor()
        {
            if (GameManager.Instance.gameType != GameType.Solo)
                yield break;
            
            float waitTime = 0;
            
            while (waitTime < GameConstants.SkillChoiceWaitTime)
            {
                waitTime += Time.deltaTime * GameManager.Instance.TimeScale;
                yield return null;
            }
            
            useSkill = false;
            bool isMine = GameManager.Instance.myPlayer == player;
            GameUIManager.Instance.HideSkillDim(isMine);
            StartCoroutine(WaitCoolTimeCor(_coolTime/2));
            
            yield return null;
        }
    }
}