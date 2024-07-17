using System;
using _Project.Scripts.Score;
using UnityEngine;

namespace  _Project.Scripts.Game.Player
{
    
    public enum InputType
    {
        Button = 0,
        Swipe = 1,
        All = 2
    }

    [CreateAssetMenu(menuName = "Create Swipe", fileName = "Swipe", order = 0)]
    public class Swipe : ScriptableObject , IInputHandler
    {
        private Vector2 _firstPos;
        public Direction currentDir;
        public float magnitude;

        public  void OnDown(Vector3 pos)
        {
            _firstPos = pos;
        }
        
        
        private bool swipeOccurred = false;

        private void SwipeAction(Direction dir , float mag)
        {
            currentDir  = dir;
            magnitude = mag;
        }

        public void OnDrag(Vector3 pos)
        {
            Vector2 swipe = (Vector2) pos - _firstPos;
            if((Vector2) pos == swipe) return;
            // Check if the swipe movement exceeds the stationary threshold
            // That means if move is in the vertical axis
               
            if (Mathf.Abs(swipe.y) > Mathf.Abs(swipe.x))
            {
                if (!swipeOccurred)
                {
                    swipeOccurred = true;
                    SwipeAction(swipe.y > 0f ? Direction.Up : Direction.Down , swipe.magnitude);
                    Debug.Log("Vertical swipe event occurred");
                }
            }
            else // If move is in the horizontal axis
            {
                if (!swipeOccurred)
                {
                    swipeOccurred = true;
                    SwipeAction(swipe.x > 0f ? Direction.Right : Direction.Left, swipe.magnitude);
                    Debug.Log("Horizontal swipe event occurred");
                }
            }
            
        }
        public void OnUp()
        {
            swipeOccurred = false; 
        }
        
    }
}