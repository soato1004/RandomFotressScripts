using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "TowerData", menuName = "ScriptableObjects/TowerData", order = 1)]
    public class TowerData : ScriptableObject
    {
        public int index = 0; // 타워 인덱스
        public string towerName = ""; // 타워이름
        public SerializableDictionaryBase<int, TowerInfo> towerInfoDic = new SerializableDictionaryBase<int, TowerInfo>();
        // public string name = ""; // 타워이름
        // public int attackPower = 0; // 타워에 한번 공격 당할때
        // public int attackSpeed = 0; // 1초당 몇번 공격하는지로
        // public int attackRange = 0; // 어느 거리안에 들어와야 공격하는지
        // public int attackType = 0; // 공격타입이 무엇인지 (단일, 다중, 스플, 뿌리기)
        // public int bulletIndex = 0; // 발사될 총알 인덱스
        // public int tier = 0; // 타워 등급
        // public int price = 0; // 구입금액 ( 특수한 루트로 금화구입시 사용 )
        // public int salePrice = 0; // 판매금액
        // public int criticalChance = 0; // 치명타확률
        // public int criticalDamage = 0; // 치명타피해. 100을 기준으로 1배.
        // public int[] dynamicData = { 0, 0 }; // 타워별로 필요한 정보를 이곳에 저장. 각 타워 클래스에 사용되는 데이터정의 기재
    }
    
    // 이름		            메모		    공격력	사정거리	공격속도	공격타입	총알타입    인덱스
    // 코끼리	    Elephant	중거리		10		800		100		1                   1
    // 드럼통	    Drum		스플레시	    10		800		100		3                   2
    // 선인장	    Sting		초장거리	    30		1500	30		1                   3
    // 불		Flame		중거리		10		500		100		1	                4
    // 기계		Machinegun	다중공격	    3		800		100		2                   5
    // 닌자		Shino		근거리		5		200		200		1                   6
    // 독		MaskMan	    뿌리기		15		800		100		4                   7
    // 특별		Swag		중거리		5		800		100		1                   8
    //
    //
    //          1단        2단           3단          4단        5단
    // 공격력		5          10           15          20         25
    // 사정거리	250(1)     500(1.5)	    800(2)	    1100(3)    1500(4)
    // 공속		1초에 1번 100
    // 공격타입    단일 1, 다중 2, 스플 3, 뿌리기 4
    // 총알타입    근접 0, 단거리 1, 중거리2, 장거리3,
}