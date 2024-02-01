using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BulletData", order = 1)]
    public class BulletData : ScriptableObject
    {
        public int index;
        public string bulletName;   //
        public string startName = "";    // 시작시 섬광 이름
        public string bodyName = "";     // 본체 이름
        public string hitName = "";      // 피격 이펙트 이름

        public BulletData() { }

        public BulletData(MonsterData other)
        {
            index = other.index;
        }
    }
    
    // 이름		            메모		    공격력	사정거리	공격속도	공격타입	총알타입    인덱스
    // 코끼리	    Elephant	중거리		10		800		100		1                   0
    // 드럼통	    Drum		스플레시	    10		800		100		3                   1
    // 선인장	    Sting		초장거리	    30		1500	30		1                   2
    // 늑대		Wolf		장거리		20		1100	50		1                   3
    // 불		Flame		중거리		10		500		100		1	                4
    // 기계		Machine		다중공격	    3		800		100		2                   5
    // 닌자		Shino		근거리		5		200		200		1                   6
    // 독		MaskMan	    뿌리기		15		800		100		4                   7
    // 특별		Swag		중거리		5		800		100		1                   8
}