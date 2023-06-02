using System;
using UnityEngine;

namespace _Project.Scripts.Match3.Actor
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] internal TileType TileType = TileType.Normal;

        public int _xIndex;
        public int _yIndex;
        
        private Board _gameBoard;
        

        public void InitTile(int x , int y ,Board board)
        {
            _xIndex = x;
            _yIndex = y;
            _gameBoard = board;
        }

        private void Start()
        {
            DisableObstacleCollider();
        }
        

        public void OnEnter()
        {
            _gameBoard.DragToTile(this);
        }

        public void OnDown()
        {
            _gameBoard.ClickTile(this);

        }
        public void OnUp()
        {
            _gameBoard.ReleaseTile();
        }
        
        private void DisableObstacleCollider()
        {
            if (TileType == TileType.Obstacle)
            {
                this.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
}
