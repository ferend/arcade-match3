using System;

namespace  _Project.Scripts.Game.Player
{
    public class ScoreController
    {
        public int _currentScore = 0;

        public event Action<int> UpdateScoreTextEvent; 
        public void AddScore(int value)
        {
            _currentScore += value;
            UpdateScoreTextEvent?.Invoke(_currentScore);
        }

        public int GetCurrentScore()
        {
            return _currentScore;
        }
        
    }
}