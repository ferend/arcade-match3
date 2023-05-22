using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Actor
{
    public class GamePiece : MonoBehaviour
    {
        private int xIndex;
        private int yIndex;
        
        private SpriteRenderer _spriteRenderer;
        private Color[] _colors = Constants.TILE_COLORS;

        private void Start()
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            SetSpriteColor();
        }
        
        private void SetSpriteColor()
        {
            
            int randomIndex = Random.Range(0, _colors.Length);
            Color randomColor = _colors[randomIndex];
            _spriteRenderer.color = randomColor;
        }

        public void SetCoord(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }
    }
}