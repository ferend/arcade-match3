using System;
using UnityEngine;

namespace _Project.Scripts.Match3.Actor
{
    public class Tile : MonoBehaviour
    {
        public int _xIndex;
        public int _yIndex;
        
        private Board _gameBoard;
        

        public void InitTile(int x , int y ,Board board)
        {
            _xIndex = x;
            _yIndex = y;
            _gameBoard = board;
        }

        public void OnMouseEnter()
        {
            _gameBoard.DragToTile(this);
        }

        public void OnMouseDown()
        {
            _gameBoard.ClickTile(this);

        }
        public void OnMouseUp()
        {
            _gameBoard.ReleaseTile();
        }
    }
}
