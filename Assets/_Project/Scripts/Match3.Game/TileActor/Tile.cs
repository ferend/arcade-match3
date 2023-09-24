using System;
using System.Collections;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.BoardActor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.TileActor
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Tile : MonoBehaviour
    {
        [FormerlySerializedAs("TileType")] [SerializeField] internal TileType tileType = TileType.Normal;

        internal int XIndex;
        internal int YIndex;
        
        private Board _gameBoard;

        private SpriteRenderer _spriteRenderer;

        public int breakableValue = 0;
        [SerializeField] private Sprite[] breakableSprites;
        [SerializeField] private Color normalColor;

        public void InitTile(int x , int y ,Board board)
        {
            XIndex = x;
            YIndex = y;
            _gameBoard = board;
            
            if (tileType == TileType.Breakable )
            {
                if (breakableSprites[breakableValue] != null)
                {
                    _spriteRenderer.sprite = breakableSprites[breakableValue];
                }
            }
 
        }

        private void Awake()
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            DisableObstacleCollider();
        }
        

        public void OnEnter() => _gameBoard.DragToTile(this);

        public void OnDown() => _gameBoard.ClickTile(this);
        public void OnUp() => _gameBoard.ReleaseTile();

        private void DisableObstacleCollider()
        {
            if (tileType == TileType.Obstacle)
            {
                this.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        public void BreakTile()
        {
            if (tileType != TileType.Breakable)
            {
                return;
            }

            StartCoroutine(BreakTileRoutine());
            
        }

        IEnumerator BreakTileRoutine()
        {
            breakableValue--;
            breakableValue = Mathf.Clamp(breakableValue, 0, breakableValue);
 
            yield return new WaitForSeconds(0.25f);
            if (breakableSprites[breakableValue] != null)
            {
                _spriteRenderer.sprite = breakableSprites[breakableValue];
            }
 
            if (breakableValue <= 0)
            {
                tileType = TileType.Normal;
                _spriteRenderer.color = normalColor;
            }
        }  
    }
}
