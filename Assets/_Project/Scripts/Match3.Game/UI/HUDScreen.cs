using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HUDScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesLeftText;
        
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

        public void UpdateScoreText(int value) => scoreText.text = value.ToString();
        public void UpdateMovesLeftText(int value) => movesLeftText.text = value.ToString();

    }
}