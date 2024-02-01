using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class SpawnManager : Common.Singleton<SpawnManager>
    {
        public enum ObjectType
        {
            Bullet, Monster, Tower, Effect, MonsterHP, DamegeText
        }
        
        public override void Reset()
        {
            JTDebug.LogColor("SpawnManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("SpawnManager Terminate");
            Destroy(Instance);
        }

        public GameObject GetDamageText(Vector3 pos)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("DamageText"),
                GameManager.Instance.game.uiEffectParent);
            go.transform.position = pos;
            go.name = "DamageTexts";
            return go;
        }
        
        public GameObject GetBullet(Vector3 pos, int bulletIndex)
        {
            // int key = (int)ObjectType.Bullet * 10000 + bulletIndex;

            BulletData data = DataManager.Instance.BulletDataDic[bulletIndex];
            
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("BulletBase"),
                GameManager.Instance.game.bulletParent);

            // 총알본체
            Instantiate(ResourceManager.Instance.GetPrefab(data.bodyName), go.transform);
            
            go.transform.position = pos;
            return go;
        }
        
        public GameObject GetTower(Vector3 pos, int towerIndex = -1)
        {
            // int key = (int)ObjectType.Tower * 10000;

            // TODO: 각 타워별로 프리팹을 나눈다
            string prefabName = "Tower";
            if (towerIndex == (int)TowerIndex.Machinegun)
            {
                TowerData data = DataManager.Instance.TowerDataDic[towerIndex];
                prefabName = data.towerName;
            }

            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName),
                GameManager.Instance.myPlayer.towerParent);
            go.name = prefabName;
            go.transform.position = pos;
            
            return go;
        }
        
        public GameObject GetEffect(string text, Vector3 pos)
        {
            // int key = (int)ObjectType.Effect * 10000 + index;

            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(text), 
                GameManager.Instance.game.effectParent);
            go.name = text;
            go.transform.position = pos;
            go.GetOrAddComponent<AutoPaticleDurationDestroy>();

            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            // int key = (int)ObjectType.Monster * 10000 + index;

            GameObject go = Instantiate(ResourceManager.Instance.GetMonster(index%12),
                GameManager.Instance.game.monsterParent);
            go.name = "Monster "+index;
            go.transform.position = pos;
            
            return go;
        }
    }
}