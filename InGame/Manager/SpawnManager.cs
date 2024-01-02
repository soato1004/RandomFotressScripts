using RandomFortress.Common.Utils;
using RandomFortress.Data;
using RandomFortress.Game;
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
        
        public HpBar GetHpBar(Vector3 pos)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("HpBar"),
                GameManager.Instance.game.uiEffectParent);
            go.transform.position = pos;
            go.name = "HpBar";
            
            return go.GetComponent<HpBar>();
        }
        
        public GameObject GetBullet(Vector3 pos, int bulletIndex)
        {
            // int key = (int)ObjectType.Bullet * 10000 + bulletIndex;

            //TODO: 총알부분 제대로 구현필요
            if (DataManager.Instance.BulletDataDic.ContainsKey(bulletIndex) == false)
                bulletIndex = 6;
            BulletData data = DataManager.Instance.BulletDataDic[bulletIndex];
            
            // 총알 감싸는 오브젝트
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("BulletBase"),
                GameManager.Instance.gameMode.bulletParent);

            // 총알본체
            GameObject body = Instantiate(ResourceManager.Instance.GetPrefab(data.bodyName), go.transform);
            body.transform.localPosition = Vector3.zero;
            
            go.transform.position = pos;
            return go;
        }
        
        public GameObject GetTower(Vector3 pos, int towerIndex = -1)
        {
            // int key = (int)ObjectType.Tower * 10000;
            
            TowerData data = DataManager.Instance.TowerDataDic[towerIndex];
            string prefabName = data.towerName;

            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName),
                GameManager.Instance.myPlayer.towerParent);
            go.name = prefabName;
            go.transform.position = pos;
            
            return go;
        }
        
        public GameObject GetEffect(string text, Vector3 pos)
        {
            // int key = (int)ObjectType.Effect * 10000 + index;

            // GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(text), 
            //     GameManager.Instance.gameMode.effectParent);
            // go.name = text;
            // go.transform.position = pos;
            // go.GetOrAddComponent<AutoPaticleDurationDestroy>();

            GameObject go = null;
            
            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            // int key = (int)ObjectType.Monster * 10000 + index;

            GameObject go = Instantiate(ResourceManager.Instance.GetMonster(index%12),
                GameManager.Instance.gameMode.monsterParent);
            go.name = "Monster "+index;
            go.transform.position = pos;
            
            return go;
        }
    }
}