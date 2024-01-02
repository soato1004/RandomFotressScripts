using Anima2D;
using RandomFortress.Constants;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class IceDebuff : DebuffBase
    {
        [SerializeField] private int slowMove; // 슬로우 퍼센트 
        [SerializeField] private int iceDuration; // 슬로우 지속시간

        private float slowParcent;
        private float elapsedTime; // 경과 시간
        private float tickInterval = 0.5f; // 데버프 틱 간격
        private Color[] originColor;
        private Material iceMaterial;
        private Material oriMaterial;
        
        public override void Init(params object[] valus)
        {
            slowMove = (int)valus[0];
            iceDuration = (int)valus[1];
            slowParcent = (100 - slowMove) / 100f;

            monster = GetComponent<MonsterBase>();
            
            iceMaterial = ResourceManager.Instance.GetMaterial(GameConstants.IceDebuffMaterial);
            oriMaterial = ResourceManager.Instance.GetMaterial(GameConstants.EmptyMaterial);
        }

        public override void UpdateDebuff()
        {
            elapsedTime += Time.deltaTime;

            if (monster != null)
            {
                monster.moveDebuff *= slowParcent;
                SetMaterial(iceMaterial);
            }

            // 디버프 종료
            if (elapsedTime >= iceDuration)
            {
                SetMaterial(oriMaterial);
                Remove();
            }
        }

        private void SetMaterial(Material material)
        {
            SpriteMeshInstance[] meshArr = GetComponentsInChildren<SpriteMeshInstance>();
            
            foreach (SpriteMeshInstance mesh in meshArr)
            {
                mesh.sharedMaterial = material;
            }
        }
        
        public override void Remove()
        {
            SpriteMeshInstance[] spriteMeshInstances = monster.GetComponentsInChildren<SpriteMeshInstance>();
            foreach (SpriteMeshInstance spriteMesh in spriteMeshInstances)
            {
                spriteMesh.color = Color.white;
            }
            
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}