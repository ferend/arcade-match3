using UnityEngine;

namespace _Project.Scripts.Match3.Actor
{
    public class Tile : MonoBehaviour
    {
        private int _xIndex;
        private int _yIndex;
        
        private Board _gameBoard;
        

        public void InitTile(int x , int y ,Board board)
        {
            _xIndex = x;
            _yIndex = y;
            _gameBoard = board;
        }
    }
}
