using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Game.Powerup;
using _Project.Scripts.Match3.Game.TileActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.BoardActor
{
    public class BoardManager : Manager
    {
        [SerializeField] private Board board;

        private bool _canGetInput = true;
        
        private readonly float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;
        
        public event Action<int, int, int> ClearPiecePfxEvent;
        public event Action<int, int, int> BombPiecePfxEvent;
        public event Action<int ,int, int, int> BreakTilePfxEvent;
        
        [Space(10)]
        [Header("Prefabs")]
        [SerializeField] private GameObject columnBombPrefab;
        [SerializeField] private GameObject colorBombPrefab;
        [SerializeField] private GameObject rowBombPrefab;
        [SerializeField] private GameObject adjacentBombPrefab;
        [SerializeField] internal GameObject tileNormalPrefab; 
        [SerializeField] internal GamePiece gamePiece;

        private Bomb _clickedTileBomb;
        private Bomb _targetTileBomb;
        
        public override void Setup()
        {
           
            _swapWaiter = new WaitForSeconds(_swapTime);
            _collapseWaiter = new WaitForSeconds(0.25f);
            
            InitTileArray();
            InitGamePieceArray();
            SetupTiles();
            SetupGamePieces();
            FillBoard();
        }
        
        private void InitTileArray() => board.TileArray = new Tile[board.Width, board.Height];
        private void InitGamePieceArray() => board.GamePieceArray = new GamePiece[board.Width, board.Height];

        private void SetupTiles()
        {
            foreach (StartingTile sTile in board.startingTiles)
            {
                if(sTile == null) return;
                CreateTile(sTile.tilePrefab,sTile.x,sTile.y);
            }
            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {
                    if (board.TileArray[i, j] == null)
                    {
                        CreateTile(tileNormalPrefab, i, j);
                    }
                }
                
            }
        }
        
        private void SetupGamePieces()
        {
            foreach (StartingTile sPiece in board.startingGamePieces)
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
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
            {
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                board.TileArray[x, y] = tile.GetComponent<Tile>();
                tile.transform.parent = board.transform;
                board.TileArray[x, y].InitTile(x, y, board);    
            }
            
        }
        
        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j <board. Height; j++)
                {
                    if (board.GamePieceArray[i, j] == null && board.TileArray[i,j].tileType != TileType.Obstacle)
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
        
        void ClearPieceAtPosition(int x, int y)
        {
            GamePiece pieceToClear = board.GamePieceArray[x, y];


            if (pieceToClear != null)
            {
                board.GamePieceArray[x, y] = null;
                Destroy(pieceToClear.gameObject);
            }
        }
        
        void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                ClearPieceAtPosition(piece.xIndex, piece.yIndex);
                if (bombedPieces.Contains(piece))
                {
                    BombPiecePfxEvent?.Invoke(piece.xIndex,piece.yIndex,0);
                }
                else
                {
                    ClearPiecePfxEvent?.Invoke(piece.xIndex,piece.yIndex,0);    
                }
            }
        }

        private void FillRandomAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
            {
                GamePiece randomPiece = Instantiate(gamePiece, Vector3.zero, Quaternion.identity);
                CreateGamePiece(randomPiece, x, y, falseYOffset);
            }
        }

        bool HasMatchOnFill(int x, int y, int minLenght = 3)
        {
            List<GamePiece> leftMatches = board.FindMatches(x, y, new Vector2(-1, 0), minLenght);
            List<GamePiece> downwardMatches = board.FindMatches(x, y, new Vector2(0, -1), minLenght);

            leftMatches = board.ListCheck(leftMatches, ref downwardMatches);

            return (leftMatches.Count > 0 || downwardMatches.Count > 0);
        }
        
        private void CreateGamePiece( GamePiece prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
            {
                prefab.SetBoard(board);
                board.PlaceGamePiece(prefab, x, y);

                if (falseYOffset != 0)
                {
                    prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                    prefab.MoveGamePiece(x, y, 0.1f);
                }

                prefab.transform.parent = board.transform;
                prefab.GetComponent<GamePiece>();
            }
        }



        private void BreakTileAt(int x , int y)
        {
            Tile tileToBreak = board.TileArray[x, y];
            if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
            {
                BreakTilePfxEvent?.Invoke(tileToBreak.breakableValue, x, y, 0);
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
        
        public void ClickTile(Tile tile)
        {
            if(board.ClickedTile == null && _canGetInput)
                board.ClickedTile = tile;
        }

        public void DragToTile(Tile tile)
        {
            if (board.ClickedTile != null &&  board.IsNextTo(tile, board.ClickedTile) && _canGetInput) 
                board.TargetTile = tile;
        }
        
        public void ReleaseTile()
        {
            if (board.ClickedTile != null && board.TargetTile != null  && _canGetInput )
            {
                StartCoroutine(SwitchTiles(board.ClickedTile,board.TargetTile, () => _canGetInput = true));
            }
            else
            {
                board.ClickedTile = null;
                board.TargetTile = null;
            }

            IEnumerator SwitchTiles(Tile current , Tile target, Action onComplete)
            {
                _canGetInput = false;
                
                GamePiece clickedPiece = board.GamePieceArray[current.XIndex, current.YIndex];
                GamePiece targetPiece = board.GamePieceArray[target.XIndex, target.YIndex];


                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(board.TargetTile.XIndex,board.TargetTile.YIndex,_swapTime); 
                    targetPiece.MoveGamePiece(board.ClickedTile.XIndex,board.ClickedTile.YIndex,_swapTime);
                    
                    yield return _swapWaiter;

                    List<GamePiece> clickedPieceMatches = CombineMatches(board.ClickedTile.XIndex, board.ClickedTile.YIndex);
                    List<GamePiece> targetPieceMatches = CombineMatches(board.TargetTile.XIndex, board.TargetTile.YIndex);
                    
                    var colorMatches = board.GetSameColorPieces(clickedPiece, targetPiece);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(board.ClickedTile.XIndex,board.ClickedTile.YIndex,_swapTime);
                        targetPiece.MoveGamePiece(board.TargetTile.XIndex,board.TargetTile.YIndex,_swapTime);
                    }
                    else
                    {
                        yield return _swapWaiter;

                        if (board.dropBombAfterMatch)
                        {
                            PerformDropBomb(clickedPieceMatches, targetPieceMatches);
                        }

                        StartCoroutine(ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList()));
                    }
                    
                    board.ClickedTile = null;
                    board.TargetTile = null;
                }

                onComplete?.Invoke();

            }

        } 
        
        private List<GamePiece> CombineMatches(int x, int y)
        {
              List<GamePiece> horMatches = board.FindHorizontalMatches(x, y);
              List<GamePiece> verMatches = board.FindVerticalMatches(x, y);
            
              horMatches ??= new List<GamePiece>();

              verMatches ??= new List<GamePiece>();

              horMatches = board.ListCheck(horMatches, ref verMatches);

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
          

        IEnumerator ClearAndRefillBoard(List<GamePiece> gamePieces)
        {
            yield return _collapseWaiter;
            
            while (true)
            {
                _canGetInput = false;

                List<GamePiece> bombedPieces = board.GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();

                // Chaining bombs 
                bombedPieces = board.GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();

                ClearPieceAt(gamePieces, bombedPieces);
                BreakTileAt(gamePieces);
                
                yield return _collapseWaiter;
                
                ActivateDroppedBomb();

                List<GamePiece> movingPieces = board.CollapseColumnByPieces(gamePieces);

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
        
        
        private Bomb CreateBomb(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
            {
                Bomb bomb = Instantiate(prefab.GetComponent<Bomb>(), new Vector3(x, y, 0), Quaternion.identity);
                bomb.SetBoard(board);
                bomb.SetCoord(x,y);
                bomb.transform.parent = this.transform;
                return bomb;
            }

            return null;
        }

        private void ActivateDroppedBomb()
        {
            if (_clickedTileBomb != null)
            {
                int x = (int)_clickedTileBomb.transform.position.x;
                int y = (int)_clickedTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
                {
                   board.GamePieceArray[x, y] = _clickedTileBomb.GetComponent<GamePiece>();
                }
                _clickedTileBomb = null;
            }

            if (_targetTileBomb != null)
            {
                int x = (int)_targetTileBomb.transform.position.x;
                int y = (int)_targetTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, board.Width, board.Height))
                {
                    board.GamePieceArray[x, y] = _targetTileBomb.GetComponent<GamePiece>();
                }
                _targetTileBomb = null;
            }
        }

        
        private void PerformDropBomb(List<GamePiece> clickedPieceMatches, List<GamePiece> targetPieceMatches)
        {
            Vector2 swipeDirection =
                new Vector2(board.TargetTile.XIndex - board.ClickedTile.XIndex, board.TargetTile.YIndex - board.ClickedTile.YIndex);
            _clickedTileBomb = DropBomb(board.ClickedTile.XIndex, board.ClickedTile.YIndex, swipeDirection, clickedPieceMatches);
            _targetTileBomb = DropBomb(board.TargetTile.XIndex, board.TargetTile.YIndex, swipeDirection, targetPieceMatches);
        }
        
        Bomb DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
        {
            Bomb bomb = null;

            if (gamePieces.Count >= board.matchCountForBombDrop)
            {
                if (board.IsCornerMatch(gamePieces))
                {
                    if (adjacentBombPrefab != null)
                    {
                        bomb = CreateBomb(adjacentBombPrefab, x, y);
                    }
                }
                else
                {
                    if (gamePieces.Count >= board.matchCountForColorBombDrop)
                    {
                        bomb = CreateBomb(colorBombPrefab, x, y);
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
            }

            return bomb;
        }


    }
}