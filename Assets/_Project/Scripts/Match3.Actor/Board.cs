using System;
using _Project.Scripts.Match3.Utility;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Match3.Actor
{
    /// <summary>
    /// Two-Dimensional array for tiles.
    /// </summary>
    
    public class Board : MonoBehaviour
    {
        private int _width = Constants.BOARD_WIDTH;
        private int _height = Constants.BOARD_HEIGHT;

        [SerializeField] private GameObject tilePrefab; 
        [SerializeField] private GamePiece gamePiece; 
        private Tile[,] _tileArray;
        private GamePiece[,] _gamePieceArray;

        private Tile _clickedTile;
        private Tile _targetTile;

        private void Start()
        {
            InitTileArray();
            InitGamePieceArray();
            CreateTiles();
            RandomFill();
        }

        private void InitTileArray() => _tileArray = new Tile[_width, _height];
        private void InitGamePieceArray() => _gamePieceArray = new GamePiece[_width, _height];

        private void CreateTiles()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0),Quaternion.identity);
                    _tileArray[i, j] = tile.GetComponent<Tile>();
                    tile.transform.parent = transform;
                    
                    _tileArray[i,j].InitTile(i,j,this);
                }
                
            }
        }

        public void PlaceGamePiece(GamePiece gamePiece, int x , int y )
        {
            if(gamePiece == null) return;
            
            gamePiece.transform.position = new Vector3(x, y, 0);
            gamePiece.transform.rotation = Quaternion.identity;
            
            if (ExtensionMethods.IsInBounds(x, y, _width, _height))
            {            
                _gamePieceArray[x, y] = gamePiece;
            }
            
            gamePiece.SetCoord(x,y);
        }

        void RandomFill()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    GamePiece randomPiece = Instantiate(gamePiece, Vector3.zero, Quaternion.identity);
                    if (randomPiece != null)
                    {
                        randomPiece.SetBoard(this);
                        PlaceGamePiece(randomPiece,i,j);
                        randomPiece.transform.parent = transform;
                    }
                }
            }
        }

        public void ClickTile(Tile tile)
        {
            if(_clickedTile == null)
                _clickedTile = tile;
        }

        public void DragToTile(Tile tile)
        {
            if (_clickedTile != null && IsNextTo(tile, _clickedTile)) 
                _targetTile = tile;
        }

        public void ReleaseTile()
        {
            if (_clickedTile != null && _targetTile != null)
            {
                SwitchTiles(_clickedTile,_targetTile);
            }
            
            void SwitchTiles(Tile current , Tile target)
            {
                GamePiece clickedPiece = _gamePieceArray[current._xIndex, current._yIndex];
                GamePiece targetPiece = _gamePieceArray[target._xIndex, target._yIndex];
                    
                clickedPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,0.5f); 
                targetPiece.MoveGamePiece(_clickedTile._xIndex,_clickedTile._yIndex,0.5f);
                Debug.Log("stay ");
                    
            }

            _clickedTile = null;
            _targetTile = null;
        }

        bool IsNextTo(Tile start , Tile end)
        {
            if (Mathf.Abs(start._xIndex - end._xIndex) == 1 && start._yIndex == end._yIndex)
            {
                return true;
            }

            if (Mathf.Abs(start._yIndex - end._yIndex) == 1 && start._xIndex == end._xIndex)
            {
                return true;
            }

            return false;
        }


        private void OnDrawGizmos()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    Handles.Label(new Vector3(i,j), "x" + i + "y" + j);
                }
                
            }
        }
    }
}
