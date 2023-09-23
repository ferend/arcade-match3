using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Game.TileActor;
using _Project.Scripts.Match3.Utility;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.BoardActor
{
    
    public class Board : MonoBehaviour
    {
        
        private int _width = Constants.BOARD_WIDTH;
        private int _height = Constants.BOARD_HEIGHT;
        private bool _canGetInput = true;

        private float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;
        
        [SerializeField] private GameObject tileNormalPrefab; 
        [SerializeField] private GamePiece gamePiece;
        [SerializeField] private StartingTile[] startingTiles;
        [SerializeField] private StartingTile[] startingGamePieces;
        
        private Tile[,] _tileArray;
        private GamePiece[,] _gamePieceArray;

        public Tile _clickedTile;
        public Tile _targetTile;
        
        public event Action<int, int, int> ClearPiecePFXEvent;
        public event Action<int ,int, int, int> BreakTilePFXEvent;


        private void Awake()
        {
            _swapWaiter = new WaitForSeconds(_swapTime);
            _collapseWaiter = new WaitForSeconds(0.25f);
        }

        private void Start()
        {
            InitTileArray();
            InitGamePieceArray();
            SetupTiles();
            SetupGamePieces();
            FillBoard();
        }

        private void InitTileArray() => _tileArray = new Tile[_width, _height];
        private void InitGamePieceArray() => _gamePieceArray = new GamePiece[_width, _height];

        private void SetupTiles()
        {
            foreach (StartingTile sTile in startingTiles)
            {
                if(sTile == null) return;
                CreateTile(sTile.tilePrefab,sTile.x,sTile.y);
            }
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (_tileArray[i, j] == null)
                    {
                        CreateTile(tileNormalPrefab, i, j);
                    }
                }
                
            }
        }

        private void SetupGamePieces()
        {
            foreach (StartingTile sPiece in startingGamePieces)
            {
                if (sPiece != null)
                {
                    GamePiece piece = Instantiate(sPiece.tilePrefab, new Vector3(sPiece.x, sPiece.y, 0),
                        Quaternion.identity).GetComponent<GamePiece>();
                    
                    CreateGamePiece(piece,sPiece.x,sPiece.y,10,0.1f);
                }
            }
        }

        private void CreateTile(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, _width, _height))
            {
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                _tileArray[x, y] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                _tileArray[x, y].InitTile(x, y, this);    
            }
            
        }
        
        private void CreateGamePiece( GamePiece prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, _width, _height))
            {
                prefab.SetBoard(this);
                PlaceGamePiece(prefab, x, y);

                if (falseYOffset != 0)
                {
                    prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                    prefab.MoveGamePiece(x, y, 0.1f);
                }

                prefab.transform.parent = transform;
                prefab.GetComponent<GamePiece>();
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

        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (_gamePieceArray[i, j] == null && _tileArray[i,j].tileType != TileType.Obstacle)
                    {
                        FillRandomAt(i, j,falseYOffset);
                        iterations = 0;

                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAtPosition(i, j);
                            FillRandomAt(i, j, falseYOffset);
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

        private void FillRandomAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, _width, _height))
            {
                GamePiece randomPiece = Instantiate(gamePiece, Vector3.zero, Quaternion.identity);
                CreateGamePiece(randomPiece, x, y, falseYOffset);
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
            if (_clickedTile != null &&  IsNextTo(tile, _clickedTile) && _canGetInput) 
                _targetTile = tile;
        }

        public void ReleaseTile()
        {
            if (_clickedTile != null && _targetTile != null  && _canGetInput )
            {
                StartCoroutine(SwitchTiles(_clickedTile,_targetTile, () => _canGetInput = true));
            }
            else
            {
                _clickedTile = null;
                _targetTile = null;
            }

            IEnumerator SwitchTiles(Tile current , Tile target, Action OnComplete)
            {
                _canGetInput = false;
                
                GamePiece clickedPiece = _gamePieceArray[current._xIndex, current._yIndex];
                GamePiece targetPiece = _gamePieceArray[target._xIndex, target._yIndex];


                if (targetPiece != null && clickedPiece != null)
                {
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
                ClearPiecePFXEvent?.Invoke(piece.xIndex,piece.yIndex,0);

            }

        }


        private void BreakTileAt(int x , int y)
        {
            Tile tileToBreak = _tileArray[x, y];
            if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
            {
                BreakTilePFXEvent?.Invoke(tileToBreak.breakableValue, x, y, 0);
                tileToBreak.BreakTile();
            }
        }

        private void BreakTileAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    BreakTileAt(piece.xIndex,piece.yIndex);
                }
            }
        }
        
        void HighlightTileOff(int x, int y)
        {
            if (_tileArray[x, y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = _tileArray[x, y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            }
        }

        void HighlightTileOn(int x, int y, Color col)
        {
            if (_tileArray[x, y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = _tileArray[x, y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = col;
            }
        }

        void ToggleHighlightPieces(List<GamePiece> gamePieces, bool highlight)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    if (highlight)
                    {
                        HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
                    }
                    else
                    {
                        HighlightTileOff(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }


        List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();

            for (int i = 0; i < _height - 1; i++)
            {
                if (_gamePieceArray[column, i] == null && _tileArray[column,i].tileType != TileType.Obstacle)
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
            //ToggleHighlightPieces(gamePieces,true);

            yield return _collapseWaiter;

            //ToggleHighlightPieces(gamePieces, false);

            while (true)
            {
                _canGetInput = false;

                ClearPieceAt(gamePieces);
                BreakTileAt(gamePieces);
                
                yield return _collapseWaiter;
                List<GamePiece> movingPieces = CollapseColumnByPieces(gamePieces);

                yield return _collapseWaiter;
                List<GamePiece> matches = CombineMatches(movingPieces);
                
                if (matches.Count == 0)
                {
                    break;
                }
                
                yield return StartCoroutine(ClearAndRefillBoard(matches));
                
            }

            yield return _collapseWaiter;
            FillBoard(10);
            _canGetInput = true;
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
