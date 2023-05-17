using System.Linq;
using UnityEngine;

namespace RandomFortress.Common.Extensions
{
    public static class TransformExtension
    {
        public static void SetActive(this Transform transform, bool active)
        {
            transform.gameObject.SetActive(active);
        }
        
        public static Transform[] GetChildren(this Transform transform)
        {
            var children = new Transform[transform.childCount];

            for (var i = 0; i < transform.childCount; i++) children[i] = transform.GetChild(i);

            return children;
        }
        
        public static void ExMoveX(this Transform t, float x)
        {
            t.position = new Vector3(t.position.x + x, t.position.y, t.position.z);
        }

        public static void ExMoveY(this Transform t, float y)
        {
            t.position = new Vector3(t.position.x, t.position.y + y, t.position.z);
        }

        public static void ExMoveZ(this Transform t, float z)
        {
            t.position = new Vector3(t.position.x, t.position.y, t.position.z + z);
        }


        public static void ExSetPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.ExNewX(x);
        }

        public static void ExSetPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.ExNewY(y);
        }

        public static void ExSetPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.ExNewZ(z);
        }

        public static void ExSetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.ExNewX(x);
        }

        public static void ExSetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.ExNewY(y);
        }

        public static void ExSetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.ExNewZ(z);
        }

        public static void ExSetLocalScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.ExNewX(x);
        }

        public static void ExSetLocalScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.ExNewY(y);
        }

        public static void ExSetLocalScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.ExNewZ(z);
        }
        
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            // 부모 오브젝트의 scale 값을 변경
            transform.localScale = globalScale;

            // 모든 자식 오브젝트의 scale 값을 변경
            foreach (Transform child in transform)
            {
                child.localScale = Vector3.Scale(child.localScale, transform.localScale);
            }
        }
        
        public static void SetGlobalScale(this Transform transform, float scale)
        {
            SetGlobalScale(transform, new Vector3(scale, scale, scale));
        }
    }

    public static class Vector3Extension
    {
        public static void ExSetX(this ref Vector3 v, float x)
        {
            v.x = x;
        }

        public static void ExSetY(this ref Vector3 v, float y)
        {
            v.y = y;
        }

        public static void ExSetZ(this ref Vector3 v, float z)
        {
            v.z = z;
        }

        public static Vector3 ExNewX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 ExNewY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 ExNewZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 ExNewXY(this Vector3 v, float xy)
        {
            return v.ExNewXY(xy, xy);
        }

        public static Vector3 ExNewXY(this Vector3 v, float x, float y)
        {
            return v.ExNewXY(new Vector2(x, y));
        }

        public static Vector3 ExNewXY(this Vector3 v, Vector2 xy)
        {
            return new Vector3(xy.x, xy.y, v.z);
        }

        public static Vector3 ExAbs(this Vector3 v)
        {
            v = v.ExAbsX();
            v = v.ExAbsY();
            v = v.ExAbsZ();
            return v;
        }

        public static Vector3 ExAbsX(this Vector3 v)
        {
            v.x = Mathf.Abs(v.x);
            return v;
        }

        public static Vector3 ExAbsY(this Vector3 v)
        {
            v.y = Mathf.Abs(v.y);
            return v;
        }

        public static Vector3 ExAbsZ(this Vector3 v)
        {
            v.z = Mathf.Abs(v.z);
            return v;
        }

        public static Vector3 ExAddXY(this Vector3 v, float x, float y)
        {
            return v.ExAddX(x).ExAddY(y);
        }

        public static Vector3 ExAddXY(this Vector3 v, float xy)
        {
            return v.ExAddX(xy).ExAddY(xy);
        }

        public static Vector3 ExAddX(this Vector3 v, float x)
        {
            v.x += x;
            return v;
        }

        public static Vector3 ExAddY(this Vector3 v, float y)
        {
            v.y += y;
            return v;
        }

        public static Vector3 ExAddZ(this Vector3 v, float z)
        {
            v.z += z;
            return v;
        }

        public static Vector3 ExReverseX(this Vector3 v) => v.ExMultiplyX(-1);

        public static Vector3 ExMultiplyX(this Vector3 v, float x)
        {
            v.x *= x;
            return v;
        }

        public static Vector3 ExMultiplyY(this Vector3 v, float y)
        {
            v.y *= y;
            return v;
        }
    }
}