using System;
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
        [SerializeField] private float elapsedTime; // 경과 시간
        
        private float slowParcent; // 슬로우 퍼센트
        private float tickInterval = 0.5f; // 데버프 틱 간격
        private Color[] originColor;
        private Material iceMaterial;
        private Material oriMaterial;
        
        public override void Init(params object[] values)
        {
            slowMove = (int)values[0];
            iceDuration = (int)values[1] / 100;
            debuffType = (DebuffType)values[2];
            debuffIndex = (DebuffIndex)values[3];
            slowParcent = (100 - slowMove) / 100f;

            monster = GetComponent<MonsterBase>();
            
            //TODO: 오브젝트풀링으로 바꾼다
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
            SetMaterial(oriMaterial);
            
            // SpriteMeshInstance[] spriteMeshInstances = monster.GetComponentsInChildren<SpriteMeshInstance>();
            // foreach (SpriteMeshInstance spriteMesh in spriteMeshInstances)
            // {
            //     spriteMesh.color = Color.white;
            // }
            
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}