using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RandomFortress.Script.Touch
{
    public class ClickableObject : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 클릭 가능여부. true 반환시 클릭 가능.
        /// </summary>
        public event Func<PointerEventData, ClickableObject, bool> ClickPredicate;

        public event Action<PointerEventData, ClickableObject> OnClick;

        protected virtual void Start()
        {
            SetUpCollider();
        }

        protected virtual void SetUpCollider()
        {
            // UI 객체가 아니라면 콜라이더 생성.
            if (!(transform is RectTransform))
            {
                if (gameObject.TryGetComponent<Collider2D>(out var myCollider2D))
                {
                    myCollider2D.isTrigger = true;
                }
                else
                {
                    gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                }
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!CanClick(eventData))
                return;

            OnClickContents(eventData);
            OnClick?.Invoke(eventData, this);
        }
        
        protected virtual bool CanClick(PointerEventData data)
        {
            if (ClickPredicate != null)
            {
                return ClickPredicate.GetInvocationList().All(func => ((Func<PointerEventData, ClickableObject, bool>) func).Invoke(data, this));
            }

            return true;
        }

        protected virtual void OnClickContents(PointerEventData eventData)
        {

        }
    }
}