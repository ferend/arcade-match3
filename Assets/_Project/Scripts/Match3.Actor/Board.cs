using System;
using _Project.Scripts.Match3.Utility;
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
        private Tile[,] _tileArray;

        private void Start()
        {
            InitTileArray();
            CreateTiles();
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
                }
                
            }
        }
    }
}
