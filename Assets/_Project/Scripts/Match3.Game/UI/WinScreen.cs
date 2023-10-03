using UnityEngine;

namespace _Project.Scripts.Match3.Game.UI
{
    
    public class WinScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public void PlayOpenAnim()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1;  
        }

        public void PlayCloseAnim()
        {
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        } 
    }
}