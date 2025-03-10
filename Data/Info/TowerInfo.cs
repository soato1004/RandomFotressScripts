using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 원본 타워데이터를 가공하여 사용할 타워데이터
    /// </summary>
    [System.Serializable]
    public class TowerInfo
    {
        [Header("TowerData")]
        public int index = 0; // 타워 인덱스
        public string towerName = ""; // 타워이름
        public int attackPower = 0; // 타워에 한번 공격 당할때
        public int attackSpeed = 0; // 1초당 몇번 공격하는지로
        public int attackRange = 0; // 어느 거리안에 들어와야 공격하는지
        public int attackType = 0; // 공격타입이 무엇인지 (단일, 다중, 스플, 뿌리기)
        public int bulletIndex = 0; // 발사될 총알 인덱스
        public int tier = 0; // 타워 등급
        public int price = 0; // 구입금액 ( 특수한 루트로 금화구입시 사용 )
        public int salePrice = 0; // 판매금액
        public ExtraInfo extraInfo; // 타워별로 필요한 정보를 이곳에 저장. 각 타워 클래스에 사용되는 데이터정의 기재

        public TowerInfo() { }
        
        public TowerInfo(TowerInfo data)
        {
            SetData(data);
        }

        private void SetData(TowerInfo data)
        {
            index = data.index;
            towerName = data.towerName;
            attackPower = data.attackPower;
            attackSpeed = data.attackSpeed;
            attackRange = data.attackRange;
            attackType = data.attackType;
            bulletIndex = data.bulletIndex;
            tier = data.tier;
            price = data.price;
            salePrice = data.salePrice;
            extraInfo = data.extraInfo;
        }

        public void UpgradeTower(TowerInfo data)
        {
            SetData(data);
        }
    }
}