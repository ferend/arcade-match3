using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _waitForSeconds;

        [SerializeField] private GameObject tilePrefab; 
        [SerializeField] private GamePiece gamePiece; 
        private Tile[,] _tileArray;
        private GamePiece[,] _gamePieceArray;

        private Tile _clickedTile;
        private Tile _targetTile;

        private void Awake()
        {
            _waitForSeconds = new WaitForSeconds(_swapTime);
        }

        private void Start()
        {
            InitTileArray();
            InitGamePieceArray();
            CreateTiles();
            RandomFill();
            //HighlightMatches();
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
                StartCoroutine(SwitchTiles(_clickedTile,_targetTile));
            }
            
            IEnumerator SwitchTiles(Tile current , Tile target)
            {
                GamePiece clickedPiece = _gamePieceArray[current._xIndex, current._yIndex];
                GamePiece targetPiece = _gamePieceArray[target._xIndex, target._yIndex];

                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,_swapTime); 
                    targetPiece.MoveGamePiece(_clickedTile._xIndex,_clickedTile._yIndex,_swapTime);

                    yield return _waitForSeconds;

                    List<GamePiece> clickedPieceMatches = CombineMatches(_clickedTile._xIndex, _clickedTile._yIndex);
                    List<GamePiece> targetPieceMatches = CombineMatches(_targetTile._xIndex, _targetTile._yIndex);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(_clickedTile._xIndex,_clickedTile._yIndex,_swapTime);
                        targetPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,_swapTime);
                    }

                    yield return _waitForSeconds;

                    HighlightMatchesAt(_clickedTile._xIndex,_clickedTile._yIndex);
                    HighlightMatchesAt(_targetTile._xIndex,_targetTile._yIndex);
                
                    _clickedTile = null;
                    _targetTile = null;
                }

            }

     
        }

        bool IsNextTo(Tile start , Tile end)
        {
            if (Mathf.Abs(start._xIndex - end._xIndex) == 1 && start._yIndex == end._yIndex)
            {
                return true;
            }

            return Mathf.Abs(start._yIndex - end._yIndex) == 1 && start._xIndex == end._xIndex;
        }

        void HighlightMatches()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    HighlightMatchesAt(i, j);
                }
            }
        }

        private void HighlightMatchesAt(int x, int y)
        {
            HighlightOff(x, y);

            var combMatches = CombineMatches(x, y);
            if (combMatches.Count > 0)
            {
                foreach (GamePiece piece in combMatches)
                {
                    HighlightOn(piece);
                }
            }
        }

        private void HighlightOn(GamePiece piece)
        {
            SpriteRenderer sr;
            sr = _tileArray[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
            sr.color = piece.GetComponent<SpriteRenderer>().color;
        }

        private void HighlightOff(int x, int y)
        {
            SpriteRenderer sr = _tileArray[x, y].GetComponent<SpriteRenderer>();

            sr.color = new Color(sr.color.r, sr.color.g, sr.color.g, 0);
        }

        private List<GamePiece> CombineMatches(int x, int y, int minLenght = 3)
        {
            List<GamePiece> horMatches = FindHorizontalMatches(x, y, 3);
            List<GamePiece> verMatches = FindVerticalMatches(x, y, 3);

            if (horMatches == null)
            {
                horMatches = new List<GamePiece>();
            }

            if (verMatches == null)
            {
                verMatches = new List<GamePiece>();
            }

            var combMatches = horMatches.Union(verMatches).ToList();
            return combMatches;
        }


        List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
        {
            List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0),2);
            List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0),2);

            rightMatches ??= new List<GamePiece>();
            leftMatches ??= new List<GamePiece>();
            
            foreach (GamePiece piece in leftMatches)
            {
                if (!rightMatches.Contains(piece))
                {
                    rightMatches.Add(piece);
                }
            }
            
            return (rightMatches.Count >= minLenght) ? rightMatches : null ;
        }
        

        List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
        {
            List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1),2);
            List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1),2);

            upwardMatches ??= new List<GamePiece>();
            downwardMatches ??= new List<GamePiece>();
            
            foreach (GamePiece piece in downwardMatches)
            {
                if (!upwardMatches.Contains(piece))
                {
                    upwardMatches.Add(piece);
                }
            }
            
            return (upwardMatches.Count >= minLenght) ? upwardMatches : null ;
        }



        List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            GamePiece startPiece = null;

            if (ExtensionMethods.IsInBounds(startX, startY,_width,_height))
            {
                startPiece = _gamePieceArray[startX, startY];
            }

            if (startPiece !=null)
            {
                matches.Add(startPiece);
            }

            else
            {
                return null;
            }

            int nextX;
            int nextY;

            int maxValue = (_width > _height) ? _width: _height;

            for (int i = 1; i < maxValue - 1; i++)
            {
                nextX = startX + (int) Mathf.Clamp(searchDirection.x,-1,1) * i;
                nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

                if (!ExtensionMethods.IsInBounds(nextX, nextY,_width,_height))
                {
                    break;
                }

                GamePiece nextPiece = _gamePieceArray[nextX, nextY];

                if (nextPiece.gamePieceColor == startPiece.gamePieceColor && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }

                else
                {
                    break;
                }
            }

            if (matches.Count >= minLength)
            {
                return matches;
            }
			
            return null;

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
