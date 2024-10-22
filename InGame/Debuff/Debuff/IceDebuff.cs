using Anima2D;


using UnityEngine;

namespace RandomFortress
{
    public class IceDebuff : DebuffBase
    {
        [SerializeField] private int slowMove; // 슬로우 퍼센트 
        
        private float slowParcent; // 슬로우 퍼센트
        private Color[] originColor;
        
        private Material oriMat;
        private Material effMat;
        
        public override void Init(params object[] values)
        {
            base.Init(values);
            
            slowMove = (int)values[1];
            debuffIndex = (DebuffIndex)values[2];
            slowParcent = (100 - slowMove) / 100f;
            
            oriMat = ResourceManager.I.GetMaterial("AllinDefaultMaterial");
            effMat = ResourceManager.I.GetMaterial("SlowMaterial");
            
            // 얼음 이펙트
            ShowDebuffEffect();
        }

        public override void UpdateDebuff()
        {
            elapsedTimer += Time.deltaTime * GameManager.I.gameSpeed;

            if (monster != null)
            {
                monster.moveDebuff *= slowParcent;
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
                        material.DisableKeyword("INNEROUTLINE_ON");
                    }
                    else
                    {
                        material.EnableKeyword("INNEROUTLINE_ON");
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

        public override void CombineDebuff()
        {
            
        }
        
        public override void Remove()
        {
            ShowDebuffEffect(true);
            
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}