using System;

namespace _Project.Scripts.Match3.Game.Player
{
    public class ScoreManager : Manager
    {
        private int _currentScore = 0;

        public event Action<int> UpdateScoreTextEvent; 
        public void AddScore(int value)
        {
            _currentScore += value;
            UpdateScoreTextEvent?.Invoke(_currentScore);
        }
        
    }
}