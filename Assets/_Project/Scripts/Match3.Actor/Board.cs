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

        private void Start()
        {
            InitTileArray();
            CreateTiles();
            RandomFill();
        }

        private void InitTileArray() => _tileArray = new Tile[_width, _height];

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

        void PlaceGamePiece(GamePiece gamePiece, int x , int y )
        {
            if(gamePiece == null) return;
            gamePiece.transform.position = new Vector3(x, y, 0);
            gamePiece.transform.rotation = Quaternion.identity;
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
                        PlaceGamePiece(randomPiece,i,j);
                    }
                }
            }
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
