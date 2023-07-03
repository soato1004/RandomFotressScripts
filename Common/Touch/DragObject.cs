using UnityEngine;
using UnityEngine.EventSystems;

namespace RandomFortress.Common.Script.Touch
{
    public class DragObject : DragEventObject
    {
        protected Vector3 dragOffset;
        protected override void OnBeginDragContents(PointerEventData eventData)
        {
            dragOffset = transform.position - ScreenToWorldPosition(eventData);
        }

        protected override void OnDragContents(PointerEventData eventData)
        {
            transform.position = ScreenToWorldPosition(eventData) + dragOffset;
        }
    }
}