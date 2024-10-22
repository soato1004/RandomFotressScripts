using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RandomFortress
{
    [DisallowMultipleComponent]
    public class DragEventObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected PointerEventData beginDragPointerEventData = null;

        /// <summary>
        /// 드래그 가능여부. true 반환시 드래그 가능.
        /// </summary>
        public event Func<PointerEventData, DragEventObject, bool> BeginDragPredicate;

        public event Func<PointerEventData, DragEventObject, bool> DragPredicate;
        public event Action<PointerEventData, DragEventObject> OnBeginDragEvent, OnDragEvent, OnEndDragEvent;

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

        protected virtual void OnDisable()
        {
            if (IsDragging())
            {
                OnEndDrag(beginDragPointerEventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanBeginDrag(eventData))
                return;

            beginDragPointerEventData = eventData;

            OnBeginDragContents(eventData);
            
            OnBeginDragEvent?.Invoke(eventData, this);
        }
        
        protected bool CanBeginDrag(PointerEventData data)
        {
            if (IsDragging())
                return false;

            if (BeginDragPredicate != null)
            {
                return BeginDragPredicate.GetInvocationList().All(func =>
                    ((Func<PointerEventData, DragEventObject, bool>) func).Invoke(data, this));
            }

            return true;
        }

        protected virtual void OnBeginDragContents(PointerEventData eventData)
        {
            
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!IsSameBeginDragPointerEventData(eventData))
                return;
            if (!CanDrag(eventData))
                return;

            OnDragContents(eventData);
            OnDragEvent?.Invoke(eventData, this);
        }

        protected bool CanDrag(PointerEventData data)
        {
            if (DragPredicate != null)
            {
                return DragPredicate.GetInvocationList().All(func =>
                    ((Func<PointerEventData, DragEventObject, bool>) func).Invoke(data, this));
            }

            return true;
        }
        
        protected virtual void OnDragContents(PointerEventData eventData)
        {
            
        }
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!IsSameBeginDragPointerEventData(eventData))
                return;

            OnEndDragContents(eventData);
            beginDragPointerEventData = null;
            OnEndDragEvent?.Invoke(eventData, this);
        }
        
        protected virtual void OnEndDragContents(PointerEventData eventData)
        {
            
        }

        public bool IsDragging() => beginDragPointerEventData != null;

        protected bool IsSameBeginDragPointerEventData(PointerEventData pointerEventData)
        {
            return beginDragPointerEventData == pointerEventData;
        }

        public Vector3 ScreenToWorldPosition(PointerEventData data)
        {
            return Utils.ScreenToWorldPoint(data);
        }
    }
}