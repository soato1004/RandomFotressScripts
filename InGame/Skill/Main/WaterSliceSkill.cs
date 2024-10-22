using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

namespace RandomFortress
{
    /// <summary>
    /// 히어로스킬. 적 몬스터 모두에게 수속성 데미지
    /// </summary>
    public class WaterSliceSkill : BaseSkill
    {
        private GameObject _prefab;
        private int _damage;
        // private int _targetCount;
        private List<ParticleSystem> particleList = new List<ParticleSystem>();

        public override void Init(int skillIndex, GamePlayer gPlayer, SkillButton button)
        {
            data =  DataManager.I.skillDataDic[skillIndex];

            _coolTime = data.coolTime;
            _damage = data.dynamicData[0];
            // _targetCount = data.dynamicData[1];
            
            player = gPlayer;
            _skillButton = button;
            skillType = SkillType.Hero;
            useSkill = false;
            _prefab = ResourceManager.I.GetPrefab("WaterSlice");
        }

        public override void SkillStart()
        {
            useSkill = true;
            UseSkill();
        }

        public override void UseSkill(params object[] _params)
        {
            StartCoroutine(UseSkillCor());
        }
        
        private IEnumerator UseSkillCor()
        {
            Debug.Log("Use SKill "+data.skillName);
            
            StartCoroutine(WaitCoolTimeCor(_coolTime));
            
            // 적 몬스터 
            var array = player.monsterList.ToArray();
            int damage = _damage * GameManager.I.myPlayer.stageProcess;

            // int count = 0;
            for (int i=0; i<array.Length; ++i)
            {
                MonsterBase monster = array[i];
                if (monster == null || monster.gameObject == null || monster.gameObject.activeSelf == false)
                    continue;

                GameObject go = SpawnManager.I.GetSkillEffect(data.skillName, monster.transform.position);

                // GameObject go = Instantiate(_prefab, SpawnManager.I.effectParent);
                // go.transform.position = monster.transform.position;
                
                // ParticleSystem particle = go.GetComponent<ParticleSystem>();
                // particle.Play();
                // particleList.Add(particle);
                monster.Hit(damage);
                // if (++count >= _targetCount)
                //      break;
                
                yield return Utils.WaitForSeconds(0.02f);
            }
            
            yield return Utils.WaitForSeconds(0.5f);
            
            GameManager.I.SetSkillDim(false,player);
            // foreach (var particle in particleList)
            //     Destroy(particle);
            
            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                player.SkillEnd(data.index);
            
        }
    }
}