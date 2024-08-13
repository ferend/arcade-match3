namespace _Project.Script.UI
{
    using UnityEngine;

    public abstract class UIElement : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }

}