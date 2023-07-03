using RandomFortress.Common.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RandomFortress.Common.Utils
{
    public static class InputUtils
    {
        /// <summary>
        /// 스크린 좌표를 월드 좌표로 전환합니다.
        /// PointerEventData 객체가 PressEvent를 받은 상태에서 호출되어야 합니다.
        /// </summary>
        public static Vector3 ScreenToWorldPoint(PointerEventData data)
        {
            var cam = data.pressEventCamera;
            if (cam == null)
                cam = Camera.main;

            // 2D orthographic camera
            if (cam.orthographic)
                return cam.ScreenToWorldPoint(data.position).ExNewZ(0f);

            // 3D perspective camera
            var pointerDrag = data.pointerDrag;
            if (pointerDrag == null)
                return Vector3.zero;

            return ScreenToWorldPointOnPlane(data.position, cam, pointerDrag.transform.position.z);
        }
	    
        /// <summary>
        /// 3D perspective camera 에서 스크린 좌표를 월드 좌표로 변환합니다.
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <param name="cam"></param>
        /// <param name="z">변환할 월드 좌표에서의 z 값</param>
        public static Vector3 ScreenToWorldPointOnPlane(Vector3 screenPosition, Camera cam, float z) 
        {
            var ray = cam.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.forward, new Vector3(0, 0, z));

            if (plane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }
    }
}