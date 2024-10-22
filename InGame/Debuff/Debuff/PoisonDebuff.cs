using Anima2D;
using UnityEngine;

namespace RandomFortress
{
    public class PoisonDebuff : DebuffBase
    {
        [SerializeField] private int poisonDamage; // 독 총 데미지

        [SerializeField] protected TowerBase attacker; // 해당 디버프를 입힌 공격자
        
        private Material effMat;
        private Material oriMat;
        
        public override void Init(params object[] values)
        {
            base.Init(values);
            
            poisonDamage = (int)values[1];
            tickInterval = (int)values[2] / 100f;
            debuffIndex = (DebuffIndex)values[3];
            attacker = (TowerBase)values[4];
            
            effMat = ResourceManager.I.GetMaterial(GameConstants.BurnDebuffMaterial);
            oriMat = ResourceManager.I.GetMaterial(GameConstants.AllinDefaultMaterial);
            
            // 화상이펙트
            ShowDebuffEffect();
        }

        public override void UpdateDebuff()
        {
            elapsedTimer += Time.deltaTime * GameManager.I.gameSpeed;
            tickTimer += Time.deltaTime * GameManager.I.gameSpeed;
            
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0;
                attacker.AddDamage(poisonDamage);
                monster?.Hit(poisonDamage, TextType.DamageBurn);
            }

            // 디버프 종료
            if (elapsedTimer >= duration)
            {
                Remove();
            }
        }

        private void ShowDebuffEffect(bool isOri = false)
        {
            if (monster.UseSpine())
            {
                MeshRenderer meshRenderer = monster.GetComponent<MeshRenderer>();

                foreach (var material in meshRenderer.materials)
                {
                    if (material == null)
                        continue;
                    
                    if (isOri)
                    {
                        material.DisableKeyword("OVERLAY_ON");
                    }
                    else
                    {
                        material.EnableKeyword("OVERLAY_ON");
                    }
                }
            }
            else
            {
                SpriteMeshInstance[] meshArr = GetComponentsInChildren<SpriteMeshInstance>();
            
                foreach (SpriteMeshInstance mesh in meshArr)
                {
                    if (isOri)
                    {
                        mesh.sharedMaterial = oriMat;
                    }
                    else
                    {
                        mesh.sharedMaterial = effMat;
                    }
                }
            }
        }
        
        public override void Remove()
        {
            // 동일버프가 있을경우 이펙트를 끄지않는다
            if (monster.GetDebuffCount(debuffIndex) == 1)
            {
                ShowDebuffEffect(true);
            }
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}