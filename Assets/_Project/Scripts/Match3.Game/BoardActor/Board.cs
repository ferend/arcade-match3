using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Game.Powerup;
using _Project.Scripts.Match3.Game.TileActor;
using _Project.Scripts.Match3.Utility;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.BoardActor
{
    
    public class Board : MonoBehaviour
    {
        [Header("Rules")]
        [SerializeField] private bool dropBombAfterMatch = false;
        [SerializeField] private int matchCountForBombDrop = 4;
        
        internal readonly int Width = Constants.BOARD_WIDTH;
        internal readonly int Height = Constants.BOARD_HEIGHT;
        private bool _canGetInput = true;
        
        private readonly float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;
        
        [Space(10)]
        [Header("Prefabs")]
        [SerializeField] private GameObject columnBombPrefab;
        [SerializeField] private GameObject rowBombPrefab;
        [SerializeField] private GameObject adjacentBombPrefab;
        [SerializeField] private GameObject tileNormalPrefab; 
        [SerializeField] private GamePiece gamePiece;
        
        [Space(10)]
        [Header("Pre-Defined Pieces")]
        [SerializeField] private StartingTile[] startingTiles;
        [SerializeField] private StartingTile[] startingGamePieces;
        
        private Tile[,] _tileArray;
        internal GamePiece[,] GamePieceArray;

        private Tile _clickedTile;
        private Tile _targetTile;
        private Bomb _clickedTileBomb;
        private Bomb _targetTileBomb;
        
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

        private void InitTileArray() => _tileArray = new Tile[Width, Height];
        private void InitGamePieceArray() => GamePieceArray = new GamePiece[Width, Height];

        private void SetupTiles()
        {
            foreach (StartingTile sTile in startingTiles)
            {
                if(sTile == null) return;
                CreateTile(sTile.tilePrefab,sTile.x,sTile.y);
            }
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
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
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, Width, Height))
            {
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                _tileArray[x, y] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                _tileArray[x, y].InitTile(x, y, this);    
            }
            
        }
        
        private void CreateGamePiece( GamePiece prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, Width, Height))
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

        private Bomb CreateBomb(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, Width, Height))
            {
                Bomb bomb = Instantiate(prefab.GetComponent<Bomb>(), new Vector3(x, y, 0), Quaternion.identity);
                bomb.SetBoard(this);
                bomb.SetCoord(x,y);
                bomb.transform.parent = this.transform;
                return bomb;
            }

            return null;
        }

        public void PlaceGamePiece(GamePiece gamePiece, int x , int y )
        {
            if(gamePiece == null) return;
            
            gamePiece.transform.position = new Vector3(x, y, 0);
            gamePiece.transform.rotation = Quaternion.identity;
            
            if (ExtensionMethods.IsInBounds(x, y, Width, Height))
            {            
                GamePieceArray[x, y] = gamePiece;
            }
            
            gamePiece.SetCoord(x,y);
        }

        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (GamePieceArray[i, j] == null && _tileArray[i,j].tileType != TileType.Obstacle)
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
            if (ExtensionMethods.IsInBounds(x, y, Width, Height))
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

            IEnumerator SwitchTiles(Tile current , Tile target, Action onComplete)
            {
                _canGetInput = false;
                
                GamePiece clickedPiece = GamePieceArray[current.XIndex, current.YIndex];
                GamePiece targetPiece = GamePieceArray[target.XIndex, target.YIndex];


                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(_targetTile.XIndex,_targetTile.YIndex,_swapTime); 
                    targetPiece.MoveGamePiece(_clickedTile.XIndex,_clickedTile.YIndex,_swapTime);
                    
                    yield return _swapWaiter;

                    List<GamePiece> clickedPieceMatches = CombineMatches(_clickedTile.XIndex, _clickedTile.YIndex);
                    List<GamePiece> targetPieceMatches = CombineMatches(_targetTile.XIndex, _targetTile.YIndex);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(_clickedTile.XIndex,_clickedTile.YIndex,_swapTime);
                        targetPiece.MoveGamePiece(_targetTile.XIndex,_targetTile.YIndex,_swapTime);
                    }
                    else
                    {
                        yield return _swapWaiter;

                        if (dropBombAfterMatch)
                        {
                            PerformDropBomb(clickedPieceMatches, targetPieceMatches);
                        }

                        StartCoroutine(ClearAndRefillBoard(clickedPieceMatches));
                        StartCoroutine(ClearAndRefillBoard(targetPieceMatches));
                        
                    }
                    
                    _clickedTile = null;
                    _targetTile = null;
                }

                onComplete?.Invoke();

            }
            
        }

        private void PerformDropBomb(List<GamePiece> clickedPieceMatches, List<GamePiece> targetPieceMatches)
        {
            Vector2 swipeDirection =
                new Vector2(_targetTile.XIndex - _clickedTile.XIndex, _targetTile.YIndex - _clickedTile.YIndex);
            _clickedTileBomb = DropBomb(_clickedTile.XIndex, _clickedTile.YIndex, swipeDirection, clickedPieceMatches);
            _targetTileBomb = DropBomb(_targetTile.XIndex, _targetTile.YIndex, swipeDirection, targetPieceMatches);
        }

        bool IsNextTo(Tile start , Tile end)
        {
            if (Mathf.Abs(start.XIndex - end.XIndex) == 1 && start.YIndex == end.YIndex)
            {
                return true;
            }

            return Mathf.Abs(start.YIndex - end.YIndex) == 1 && start.XIndex == end.XIndex;
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

            if (ExtensionMethods.IsInBounds(startX, startY,Width,Height))
            {
                startPiece = GamePieceArray[startX, startY];
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

            int maxValue = (Width > Height) ? Width: Height;

            for (int i = 1; i < maxValue - 1; i++)
            {
                nextX = startX + (int) Mathf.Clamp(searchDirection.x,-1,1) * i;
                nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

                if (!ExtensionMethods.IsInBounds(nextX, nextY,Width,Height))
                {
                    break;
                }

                GamePiece nextPiece = GamePieceArray[nextX, nextY];

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
            GamePiece pieceToClear = GamePieceArray[x, y];


            if (pieceToClear != null)
            {
                GamePieceArray[x, y] = null;
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

            for (int i = 0; i < Height - 1; i++)
            {
                if (GamePieceArray[column, i] == null && _tileArray[column,i].tileType != TileType.Obstacle)
                {
                    for (int j = i + 1; j < Height; j++)
                    {
                        if (GamePieceArray[column, j] != null)
                        {
                            GamePieceArray[column, j].MoveGamePiece(column, i, collapseTime * (j - i) );
                            GamePieceArray[column, i] = GamePieceArray[column, j];
                            GamePieceArray[column, i].SetCoord(column, i);

                            if (!movingPieces.Contains(GamePieceArray[column, i]))
                            {
                                movingPieces.Add(GamePieceArray[column, i]);
                            }

                            GamePieceArray[column, j] = null;
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

                List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();
                
                ClearPieceAt(gamePieces);
                BreakTileAt(gamePieces);
                
                yield return _collapseWaiter;
                
                ActivateDroppedBomb();

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
        
        private List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
        {
            List<GamePiece> allPiecesToClear = new List<GamePiece>();

            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    List<GamePiece> piecesToClear = new List<GamePiece>();
                    Bomb bomb = piece.GetComponent<Bomb>();
                    if (bomb != null)
                    {
                        piecesToClear = bomb.bombType switch
                        {
                            BombType.Column => bomb.GetColumnPieces(bomb.xIndex),
                            BombType.Row => bomb.GetRowPieces(bomb.yIndex),
                            BombType.Adjacent => bomb.GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1),
                            _ => piecesToClear
                        };
                    }

                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                }
            }

            return allPiecesToClear;
        }

        private bool IsCornerMatch(List<GamePiece> gamePieces)
        {
            bool vertical = false;
            bool horizontal = false;
            int xStart = -1;
            int yStart = -1;

            foreach (GamePiece piece in gamePieces)
            {
                if (piece == null) continue;
                if (xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if (piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }

                if (piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }

            return (horizontal && vertical);

        }

        Bomb DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
        {
            Bomb bomb = null;

            if (gamePieces.Count >= matchCountForBombDrop)
            {
                if (IsCornerMatch(gamePieces))
                {
                    if (adjacentBombPrefab != null)
                    {
                        bomb = CreateBomb(adjacentBombPrefab, x, y);
                    }
                }
                else
                {
                    if (swapDirection.x != 0)
                    {
                        if (rowBombPrefab != null)
                        {
                            bomb = CreateBomb(rowBombPrefab, x, y);
                        }
                    }
                    else
                    {
                        if (columnBombPrefab != null)
                        {
                            bomb = CreateBomb(columnBombPrefab, x, y);
                        }
                    }
                }
            }

            return bomb;
        }
        
        private void ActivateDroppedBomb()
        {
            if (_clickedTileBomb != null)
            {
                int x = (int)_clickedTileBomb.transform.position.x;
                int y = (int)_clickedTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, Width, Height))
                {
                    GamePieceArray[x, y] = _clickedTileBomb.GetComponent<GamePiece>();
                }
                _clickedTileBomb = null;
            }

            if (_targetTileBomb != null)
            {
                int x = (int)_targetTileBomb.transform.position.x;
                int y = (int)_targetTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, Width, Height))
                {
                    GamePieceArray[x, y] = _targetTileBomb.GetComponent<GamePiece>();
                }
                _targetTileBomb = null;
            }
        }
        
        private void OnDrawGizmos()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Handles.Label(new Vector3(i,j), "x" + i + "y" + j);
                }
                
            }
        }
    }
}
