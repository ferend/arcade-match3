using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.Collectibles;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Game.Powerup;
using _Project.Scripts.Match3.Game.TileActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Match3.Game.BoardActor
{
    public class BoardManager : Manager
    {
        [SerializeField] private Board board;
        [SerializeField] private GamePiece gamePiece;

        private bool _canGetInput = true;
        
        private readonly float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;
        
        public event Action<int, int, int> ClearPiecePfxEvent;
        public event Action<int, int, int> BombPiecePfxEvent;
        public event Action<int ,int, int, int> BreakTilePfxEvent;
        public event Action<int> AddScoreEvent;
        
        [Space(10)]
        [Header("Prefabs")]
        [SerializeField] private GameObject columnBombPrefab;
        [SerializeField] private GameObject colorBombPrefab;
        [SerializeField] private GameObject rowBombPrefab;
        [SerializeField] private GameObject adjacentBombPrefab;
        [SerializeField] private GameObject tileNormalPrefab; 
        [SerializeField] private GameObject[] collectiblePrefabs; 

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

            List<GamePiece> startingCollectibles = FindAllCollectibles();
            board.collectibleCount = startingCollectibles.Count;

            board.removeCollectibleDelegate = RemoveCollectibles;

        }
        
        private void InitTileArray() => board.tileArray = new Tile[board.width, board.height];
        private void InitGamePieceArray() => board.gamePieceArray = new GamePiece[board.width, board.height];

        private void SetupTiles()
        {
            foreach (StartingTile sTile in board.startingTiles)
            {
                if(sTile == null) return;
                CreateTile(sTile.tilePrefab,sTile.x,sTile.y);
            }
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j < board.height; j++)
                {
                    if (board.tileArray[i, j] == null)
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
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.width, board.height))
            {
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                board.tileArray[x, y] = tile.GetComponent<Tile>();
                tile.transform.parent = board.transform;
                board.tileArray[x, y].InitTile(x, y, board);    
            }
            
        }
        
        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j <board. height; j++)
                {
                    if (board.gamePieceArray[i, j] == null && board.tileArray[i,j].tileType != TileType.Obstacle)
                    {

                        if (j == board.height - 1 && CanAddCollectible())
                        {
                            FillRandomCollectibleAt(i,j, falseYOffset);
                            board.collectibleCount++;
                        }
                        else
                        {
                            FillRandomGamePieceAt(i, j,falseYOffset);
                            iterations = 0;

                            while (HasMatchOnFill(i, j))
                            {
                                ClearPieceAtPosition(i, j);
                                FillRandomGamePieceAt(i, j, falseYOffset);
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
        }
        
        private void ClearPieceAtPosition(int x, int y)
        {
            GamePiece pieceToClear = board.gamePieceArray[x, y];
            
            if (pieceToClear != null)
            {
                board.gamePieceArray[x, y] = null;
                Destroy(pieceToClear.gameObject);
            }
        }
        
        private void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                ClearPieceAtPosition(piece.xIndex, piece.yIndex);
                
                AddScoreEvent?.Invoke(piece.scoreValue);
                
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

        private GamePiece GetRandomCollectible() => GetRandomObject(collectiblePrefabs);

        private GamePiece GetRandomObject(GameObject[] objectArray)
        {
            int randomIndex = Random.Range(0, objectArray.Length);

            if (objectArray[randomIndex] == null) Debug.Log("No valid game object at index");

            return objectArray[randomIndex].GetComponent<GamePiece>();
        }
        
        private void FillRandomCollectibleAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, board.width, board.height))
            {
                GamePiece randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
                CreateGamePiece(randomPiece, x, y, falseYOffset);
            }
        }

        private void FillRandomGamePieceAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, board.width, board.height))
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
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.width, board.height))
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
            Tile tileToBreak = board.tileArray[x, y];
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
            if(board.clickedTile == null && _canGetInput)
                board.clickedTile = tile;
        }

        public void DragToTile(Tile tile)
        {
            if (board.clickedTile != null &&  board.IsNextTo(tile, board.clickedTile) && _canGetInput) 
                board.targetTile = tile;
        }
        
        public void ReleaseTile()
        {
            if (board.clickedTile != null && board.targetTile != null  && _canGetInput )
            {
                StartCoroutine(SwitchTiles(board.clickedTile,board.targetTile, () => _canGetInput = true));
            }
            else
            {
                board.clickedTile = null;
                board.targetTile = null;
            }

            IEnumerator SwitchTiles(Tile current , Tile target, Action onComplete)
            {
                _canGetInput = false;
                
                GamePiece clickedPiece = board.gamePieceArray[current.XIndex, current.YIndex];
                GamePiece targetPiece = board.gamePieceArray[target.XIndex, target.YIndex];


                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(board.targetTile.XIndex,board.targetTile.YIndex,_swapTime); 
                    targetPiece.MoveGamePiece(board.clickedTile.XIndex,board.clickedTile.YIndex,_swapTime);
                    
                    yield return _swapWaiter;

                    List<GamePiece> clickedPieceMatches = CombineMatches(board.clickedTile.XIndex, board.clickedTile.YIndex);
                    List<GamePiece> targetPieceMatches = CombineMatches(board.targetTile.XIndex, board.targetTile.YIndex);
                    
                    var colorMatches = board.GetSameColorPieces(clickedPiece, targetPiece);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(board.clickedTile.XIndex,board.clickedTile.YIndex,_swapTime);
                        targetPiece.MoveGamePiece(board.targetTile.XIndex,board.targetTile.YIndex,_swapTime);
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
                    
                    board.clickedTile = null;
                    board.targetTile = null;
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

                List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);
                List<GamePiece> allCollectibles = FindAllCollectibles();
                List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
                collectedPieces = collectedPieces.Union(blockers).ToList();
                board.collectibleCount -= collectedPieces.Count;

                gamePieces = gamePieces.Union(collectedPieces).ToList();

                ClearPieceAt(gamePieces, bombedPieces);
                BreakTileAt(gamePieces);
                
                yield return _collapseWaiter;
                
                ActivateDroppedBomb();

                List<GamePiece> movingPieces = board.CollapseColumnByPieces(gamePieces);

                yield return _collapseWaiter;
                List<GamePiece> matches = CombineMatches(movingPieces);
                
                // Check bottom row for collectibles. Fix waiting issue
                collectedPieces = FindCollectiblesAt(0, true);
                matches = matches.Union(collectedPieces).ToList();

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
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, board.width, board.height))
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

                if (ExtensionMethods.IsInBounds(x, y, board.width, board.height))
                {
                   board.gamePieceArray[x, y] = _clickedTileBomb.GetComponent<GamePiece>();
                }
                _clickedTileBomb = null;
            }

            if (_targetTileBomb != null)
            {
                int x = (int)_targetTileBomb.transform.position.x;
                int y = (int)_targetTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, board.width, board.height))
                {
                    board.gamePieceArray[x, y] = _targetTileBomb.GetComponent<GamePiece>();
                }
                _targetTileBomb = null;
            }
        }

        
        private void PerformDropBomb(List<GamePiece> clickedPieceMatches, List<GamePiece> targetPieceMatches)
        {
            Vector2 swipeDirection =
                new Vector2(board.targetTile.XIndex - board.clickedTile.XIndex, board.targetTile.YIndex - board.clickedTile.YIndex);
            _clickedTileBomb = DropBomb(board.clickedTile.XIndex, board.clickedTile.YIndex, swipeDirection, clickedPieceMatches);
            _targetTileBomb = DropBomb(board.targetTile.XIndex, board.targetTile.YIndex, swipeDirection, targetPieceMatches);
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
        
        private List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces)
        {
            List<GamePiece> collectiblePieces = FindAllCollectibles();
            List<GamePiece> piecesToRemove = new List<GamePiece>();

            foreach (GamePiece piece in collectiblePieces)
            {
                Collectible collectible = piece.GetComponent<Collectible>();

                if (collectible != null)
                {
                    if (!collectible.clearedByBomb)
                    {
                        piecesToRemove.Add(piece);
                    }
                }
            }

            return bombedPieces.Except(piecesToRemove).ToList();

        }
        
        private List<GamePiece> FindAllCollectibles()
        {
            List<GamePiece> foundCollectibles = new List<GamePiece>();

            for (int i = 0; i < board.height; i++)
            {
                List<GamePiece> collectibleRow = FindCollectiblesAt(i);
                foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
            }

            return foundCollectibles;
        }
        

        private List<GamePiece> FindCollectiblesAt(int row, bool collectedAtBottom = false)
        {
            List<GamePiece> foundCollectibles = new List<GamePiece>();

            for (int i = 0; i < board.width; i++)
            {
                if (board.gamePieceArray[i, row] != null)
                {
                    Collectible collectible = board.gamePieceArray[i, row].GetComponent<Collectible>();

                    if (collectible != null)
                    {
                        if (!collectedAtBottom || (collectedAtBottom && collectible.clearedAtBottom))
                        {
                            foundCollectibles.Add(board.gamePieceArray[i,row]);
                        }
                    }
                }
            }

            return foundCollectibles;
        }

        bool CanAddCollectible()
        {
            return (Random.Range(0f, 1f) <= board.chanceForCollectible && collectiblePrefabs.Length > 0 &&
                    board.collectibleCount < board.maxCollectibleCount);
        }


    }
}