using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Player
{
    public class ScoreManager : Manager
    {
        private int _currentScore = 0;
        
        [SerializeField] private TextMeshProUGUI scoreText;
        
        public void AddScore(int value)
        {
            _currentScore += value;
            UpdateScoreText(_currentScore);
        }
        
        private void UpdateScoreText(int value) => scoreText.text = value.ToString();
        
    }
}