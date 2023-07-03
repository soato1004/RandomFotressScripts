using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "TowerData", menuName = "ScriptableObjects/TowerData", order = 1)]
    public class TowerData : ScriptableObject
    {
        public int index; // 타워 인덱스
        public string towerName; // 타워이름
        public int attackPower; // 타워에 한번 공격 당할때
        public int attackSpeed; // 1초당 몇번 공격하는지로
        public int attackRange; // 어느 거리안에 들어와야 공격하는지
        public int attackType; // 공격타입이 무엇인지 (단일, 다중, 스플, 뿌리기)
        public int bulletIndex; // 발사될 총알 인덱스 (근접일경우 0)
        public int tier; // 타워 등급
        public int price; // 구입금액 ( 특수한 루트로 금화구입시 사용 )
        public int salePrice; // 판매금액

        public TowerData() { }

        public TowerData(TowerData other)
        {
            index = other.index;
            towerName = other.towerName;
            attackPower= other.attackPower; 
            attackSpeed= other.attackSpeed;
            attackRange= other.attackRange;
            attackType= other.attackType; 
            bulletIndex= other.bulletIndex;
            tier= other.tier; 
            price= other.price; 
            salePrice= other.salePrice; 
        }
    }
    
    // 이름		            메모		    공격력	사정거리	공격속도	공격타입	총알타입
    // 코끼리	    Elephant	중거리		10		500		100		1
    // 드럼통	    Drum		스플레시	    10		500		100		3
    // 선인장	    Sting		초장거리	    30		1100	30		1
    // 늑대		Wolf		장거리		20		800		50		1
    // 불		Rider		중거리		10		500		100		1	
    // 기계		Bomb		다중공격	    3		500		100		2
    // 닌자		Shino		근거리		5		200		200		1
    // 독		MaskMan	    지속형		15		500		100		4
    // 특별		Swag		중거리		5		500		100		1
    //
    //
    //
    // 공격력		5	10	15	20	25
    // 사정거리	    200	500	800	1100
    // 공속		    1초에 1번 100
    // 공격타입     단일 1, 다중 2, 스플 3, 뿌리기 4
    // 총알타입     근접 0, 단거리 1, 중거리2, 장거리3,
}