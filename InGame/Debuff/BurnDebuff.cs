using Anima2D;


using UnityEngine;

namespace RandomFortress
{
    public class BurnDebuff : DebuffBase
    {
        [SerializeField] protected int burnDamage; // 틱 화상 데미지
        [SerializeField] private float elapsedTime; // 경과 시간
        [SerializeField] private float tickInterval; // 데버프 틱 간격

        private float tickTimer = 0;
        
        private float slowParcent; // 슬로우 퍼센트
        private Color[] originColor;

        private Shader _shader;
        
        private Material effMat;
        private Material oriMat;
        
        public override void Init(params object[] values)
        {
            base.Init(values);
            
            burnDamage = (int)values[1];
            tickInterval = (int)values[2] / 100f;
            debuffIndex = (DebuffIndex)values[3];
            
            effMat = ResourceManager.Instance.GetMaterial(GameConstants.BurnDebuffMaterial);
            oriMat = ResourceManager.Instance.GetMaterial(GameConstants.AllinDefaultMaterial);
            
            // 화상이펙트
            ShowDebuffEffect();
        }

        public override void UpdateDebuff()
        {
            elapsedTime += Time.deltaTime * GameManager.Instance.TimeScale;
            tickTimer += Time.deltaTime * GameManager.Instance.TimeScale;

            // 1틱을 0.2초로 정의한다
            if (tickTimer >= GameConstants.DebuffTickTime)
            {
                tickTimer = 0;
                
                if (monster != null)
                {
                    monster.Hit(burnDamage, TextType.DamageBurn);
                }
            }

            // 디버프 종료
            if (elapsedTime >= duration)
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
            ShowDebuffEffect(true);
            
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}