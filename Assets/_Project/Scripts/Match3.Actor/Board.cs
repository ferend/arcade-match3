using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Utility;
using Unity.Plastic.Antlr3.Runtime.Misc;
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
        public bool _canGetInput = true;

        private float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;

        [SerializeField] private GameObject tilePrefab; 
        [SerializeField] private GamePiece gamePiece; 
        private Tile[,] _tileArray;
        private GamePiece[,] _gamePieceArray;

        private Tile _clickedTile;
        private Tile _targetTile;

        private void Awake()
        {
            _swapWaiter = new WaitForSeconds(_swapTime);
            _collapseWaiter = new WaitForSeconds(0.25f);
            
        }

        private void Start()
        {
            InitTileArray();
            InitGamePieceArray();
            CreateTiles();
            FillBoard();
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

        void FillBoard()
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (_gamePieceArray[i, j] == null)
                    {
                        FillRandomAt(i, j);
                        iterations = 0;

                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAtPosition(i, j);
                            FillRandomAt(i, j);
                            iterations++;

                            if (iterations >= maxIterations)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void FillRandomAt(int x, int y)
        {
            GamePiece randomPiece = Instantiate(gamePiece, Vector3.zero, Quaternion.identity);
            if (randomPiece != null)
            {
                randomPiece.SetBoard(this);
                PlaceGamePiece(randomPiece, x, y);
                randomPiece.transform.parent = transform;
                randomPiece.GetComponent<GamePiece>();
            }
        }

        bool HasMatchOnFill(int x, int y, int minLenght = 3)
        {
            List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLenght);
            List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLenght);

            leftMatches = ListCheck(leftMatches, ref downwardMatches);

            return (leftMatches.Count > 0 || downwardMatches.Count > 0);
        }


        public void ClickTile(Tile tile)
        {
            if(_clickedTile == null && _canGetInput)
                _clickedTile = tile;
        }

        public void DragToTile(Tile tile)
        {
            if (_clickedTile != null && IsNextTo(tile, _clickedTile) && _canGetInput) 
                _targetTile = tile;
        }

        public void ReleaseTile()
        {
            if (_clickedTile != null && _targetTile != null  && _canGetInput )
            {
                StartCoroutine(SwitchTiles(_clickedTile,_targetTile, () => _canGetInput = true));
            }
            
            IEnumerator SwitchTiles(Tile current , Tile target, Action OnComplete)
            {
                _canGetInput = false;
                
                GamePiece clickedPiece = _gamePieceArray[current._xIndex, current._yIndex];
                GamePiece targetPiece = _gamePieceArray[target._xIndex, target._yIndex];


                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,_swapTime); 
                    clickedPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,_swapTime); 
                    targetPiece.MoveGamePiece(_clickedTile._xIndex,_clickedTile._yIndex,_swapTime);
                    
                    yield return _swapWaiter;

                    List<GamePiece> clickedPieceMatches = CombineMatches(_clickedTile._xIndex, _clickedTile._yIndex);
                    List<GamePiece> targetPieceMatches = CombineMatches(_targetTile._xIndex, _targetTile._yIndex);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(_clickedTile._xIndex,_clickedTile._yIndex,_swapTime);
                        targetPiece.MoveGamePiece(_targetTile._xIndex,_targetTile._yIndex,_swapTime);
                    }
                    else
                    {
                        yield return _swapWaiter;

                        StartCoroutine(ClearAndRefillBoard(clickedPieceMatches));
                        StartCoroutine(ClearAndRefillBoard(targetPieceMatches));
                    }
                    
                    _clickedTile = null;
                    _targetTile = null;
                }

                OnComplete?.Invoke();

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

        
        private List<GamePiece> CombineMatches(int x, int y)
        {
            List<GamePiece> horMatches = FindHorizontalMatches(x, y);
            List<GamePiece> verMatches = FindVerticalMatches(x, y);
            
            if (horMatches == null)
            {
                horMatches = new List<GamePiece>();
            }

            if (verMatches == null)
            {
                verMatches = new List<GamePiece>();
            }

            horMatches = ListCheck(horMatches, ref verMatches);

            var combMatches = horMatches.Union(verMatches).ToList();
            return combMatches;
        }
        
        List<GamePiece> CombineMatches(List<GamePiece> gamePieces)
        {
            List<GamePiece> matches = new List<GamePiece>();

            foreach (GamePiece piece in gamePieces)
            {
                if (piece == null) continue;
                matches = matches.Union(CombineMatches(piece.xIndex, piece.yIndex)).ToList();
            }

            return matches;
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

                if (nextPiece == null)
                {
                    break;
                }

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
        
        private List<GamePiece> ListCheck(List<GamePiece> leftMatches, ref List<GamePiece> downwardMatches)
        {
            if (leftMatches == null)
            {
                leftMatches = new List<GamePiece>();
            }

            if (downwardMatches == null)
            {
                downwardMatches = new List<GamePiece>();
            }

            return leftMatches;
        }


        void ClearPieceAtPosition(int x, int y)
        {
            GamePiece pieceToClear = _gamePieceArray[x, y];

            if (pieceToClear != null)
            {
                _gamePieceArray[x, y] = null;
                Destroy(pieceToClear.gameObject);
            }
        }
        
        void ClearPieceAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                ClearPieceAtPosition(piece.xIndex, piece.yIndex);
            }
        }

        List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();

            for (int i = 0; i < _height - 1; i++)
            {
                if (_gamePieceArray[column, i] == null)
                {
                    for (int j = i + 1; j < _height; j++)
                    {
                        if (_gamePieceArray[column, j] != null)
                        {
                            _gamePieceArray[column, j].MoveGamePiece(column, i, collapseTime * (j - i) );
                            _gamePieceArray[column, i] = _gamePieceArray[column, j];
                            _gamePieceArray[column, i].SetCoord(column, i);

                            if (!movingPieces.Contains(_gamePieceArray[column, i]))
                            {
                                movingPieces.Add(_gamePieceArray[column, i]);
                            }

                            _gamePieceArray[column, j] = null;
                            break;

                        }
                    }
                }
            }
            return movingPieces;

        }

        List<GamePiece> CollapseColumnByPieces(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<int> columnsToCollapse = GetColumns(gamePieces);

            foreach (int column in columnsToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
            }

            return movingPieces;
        }

        List<int> GetColumns(List<GamePiece> gamePieces)
        {
            List<int> columns = new List<int>();

            foreach (GamePiece piece in gamePieces)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }

            return columns;
        }

        IEnumerator ClearAndRefillBoard(List<GamePiece> gamePieces)
        {
            StartCoroutine(ClearAndCollapse(gamePieces));
            yield return null;
        }

        IEnumerator ClearAndCollapse(List<GamePiece> gamePieces)
        {
            yield return _collapseWaiter;
            
            while (true)
            {
                _canGetInput = false;

                ClearPieceAt(gamePieces);
                
                yield return _collapseWaiter;
                List<GamePiece> movingPieces = CollapseColumnByPieces(gamePieces);

                yield return _collapseWaiter;
                List<GamePiece> matches = CombineMatches(movingPieces);
                
                if (matches.Count == 0)
                {
                    break;
                }
                
                yield return StartCoroutine(ClearAndCollapse(matches));

            }

            yield return _collapseWaiter;
            StartCoroutine(RefillBoard());
            _canGetInput = true;
        }

        IEnumerator RefillBoard()
        {
            FillBoard();
            yield return null;
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
