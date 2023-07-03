using System;
using UnityEngine;

namespace RandomFortress.Common.Extensions
{
    public static class ComponentExtension
    {
        public static T ExGetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();
            return component;
        }

        public static void ExSetActive(this Component component, bool value)
        {
            component.gameObject.SetActive(value);
        }

        public static void ExRadioActiveInSiblings(this Component component, bool value)
        {
            component.ExActionInSiblings<Transform>(sibling => sibling.ExSetActive(sibling == component.transform ? value : !value));
        }

        public static void ExActionInSiblings<T>(this Component component, Action<T> action)
        {
            var parent = component.transform.parent;
            for (var i = 0; i < parent.childCount; i++)
            {
                var type = parent.GetChild(i).GetComponent<T>();
                if (type != null) action?.Invoke(type);
            }
        }
    }
}