using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Script.UI
{
    public class UISystem : Game.System
    {
        private Dictionary<System.Type, UIElement> uiElements = new Dictionary<System.Type, UIElement>();

        private void Awake()
        {
            ServiceLocator.RegisterService(this); 

            UIElement[] elements = GetComponentsInChildren<UIElement>(true);
            foreach (UIElement element in elements)
            {
                uiElements[element.GetType()] = element;
                element.Hide(); // Hide all UI elements initially
            }
        }

        public void Show<T>() where T : UIElement
        {
            if (uiElements.TryGetValue(typeof(T), out UIElement element))
            {
                element.Show();
            }
        }

        public void Hide<T>() where T : UIElement
        {
            if (uiElements.TryGetValue(typeof(T), out UIElement element))
            {
                element.Hide();
            }
        }

        public void Toggle<T>() where T : UIElement
        {
            if (uiElements.TryGetValue(typeof(T), out UIElement element))
            {
                if (element.gameObject.activeSelf)
                {
                    element.Hide();
                }
                else
                {
                    element.Show();
                }
            }
        }
    }

}

