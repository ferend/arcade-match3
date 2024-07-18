using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Core;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.Tile;
using _Project.Scripts.Game.TileActor;
using _Project.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Game.Board
{
    public class BoardManager : Manager
    {
        [SerializeField] internal BoardComponent boardComponent;
        [SerializeField] private BaseGamePiece baseGamePiece;

        public bool canGetInput = true;
        public bool canDropTiles = true;
        
        private readonly float _swapTime = Constants.TILE_SWAP_TIME;
        private WaitForSeconds _swapWaiter;
        private WaitForSeconds _collapseWaiter;
        
        public event Action<int, int, int> ClearPiecePfxEvent;
        public event Action<int, int, int> BombPiecePfxEvent;
        public event Action<int ,int, int, int> BreakTilePfxEvent;
        public event Action PlayPopSoundEvent;
        public event Action PlayBombSoundEvent;
        
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

        private void Awake()
        {
            boardComponent.gameObject.SetActive(true);
            boardComponent.SetupBoardComponent();
        }

        public override void Setup()
        {
            _swapWaiter = new WaitForSeconds(_swapTime);
            _collapseWaiter = new WaitForSeconds(0.25f);

            SetupTiles();
            SetupGamePieces();
            FillBoard();
            
            List<BaseGamePiece> startingCollectibles = FindAllCollectibles();
            boardComponent.levelData.collectibleCount = startingCollectibles.Count;
        }
  
        private void SetupTiles()
        {
            foreach (StartingTile sTile in boardComponent.levelData.startingTiles)
            {
                if(sTile == null) return;
                CreateTile(sTile.tilePrefab,sTile.x,sTile.y);
            }
            for (int i = 0; i < boardComponent.width; i++)
            {
                for (int j = 0; j < boardComponent.height; j++)
                {
                    if (boardComponent.tileArray[i, j] == null)
                    {
                        CreateTile(tileNormalPrefab, i, j);
                    }
                }
                
            }
        }
        
        private void SetupGamePieces()
        {
            foreach (StartingTile sPiece in boardComponent.levelData.startingGamePieces)
            {
                if (sPiece != null)
                {
                    BaseGamePiece piece = Instantiate(sPiece.tilePrefab, new Vector3(sPiece.x, sPiece.y, 0),
                        Quaternion.identity).GetComponent<BaseGamePiece>();
                    
                    CreateGamePiece(piece,sPiece.x,sPiece.y,10,0.1f);
                }
            }
        }
        
        private void CreateTile(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                boardComponent.tileArray[x, y] = tile.GetComponent<TileComponent>();
                tile.transform.parent = boardComponent.transform;
                boardComponent.tileArray[x, y].InitTile(x, y, boardComponent);    
            }
            
        }
        
        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIterations = 100;
            int iterations;
      
            for (int i = 0; i < boardComponent.width; i++)
            {
                for (int j = 0; j <boardComponent. height; j++)
                {
                    if (boardComponent.gamePieceArray[i, j] == null && boardComponent.tileArray[i,j].tileType != TileType.Obstacle)
                    {

                        if (j == boardComponent.height - 1 && CanAddCollectible())
                        {
                            FillRandomCollectibleAt(i,j, falseYOffset);
                            boardComponent.levelData.collectibleCount++;
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
            BaseGamePiece pieceToClear = boardComponent.gamePieceArray[x, y];
            
            if (pieceToClear != null)
            {
                boardComponent.gamePieceArray[x, y] = null;
                Destroy(pieceToClear.gameObject);
            }
        }
        
        private void ClearPieceAt(List<BaseGamePiece> gamePieces, List<BaseGamePiece> bombedPieces)
        {
            foreach (BaseGamePiece piece in gamePieces)
            {
                PlayPopSoundEvent?.Invoke();

                ClearPieceAtPosition(piece.xIndex, piece.yIndex);
                
                if (bombedPieces.Contains(piece))
                {
                    BombPiecePfxEvent?.Invoke(piece.xIndex,piece.yIndex,0);
                    PlayBombSoundEvent?.Invoke();
                }
                else
                {
                    ClearPiecePfxEvent?.Invoke(piece.xIndex,piece.yIndex,0);    
                }
            }
        }

        private BaseGamePiece GetRandomCollectible() => GetRandomObject(collectiblePrefabs);

        private BaseGamePiece GetRandomObject(GameObject[] objectArray)
        {
            int randomIndex = Random.Range(0, objectArray.Length);

            if (objectArray[randomIndex] == null) Debug.Log("No valid game object at index");

            return objectArray[randomIndex].GetComponent<BaseGamePiece>();
        }
        
        private void FillRandomCollectibleAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                BaseGamePiece randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
                CreateGamePiece(randomPiece, x, y, falseYOffset);
            }
        }

        private void FillRandomGamePieceAt(int x, int y, int falseYOffset = 0 )
        {
            if (ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                BaseGamePiece randomPiece = Instantiate(baseGamePiece, Vector3.zero, Quaternion.identity);
                CreateGamePiece(randomPiece, x, y, falseYOffset);
            }
        }

        bool HasMatchOnFill(int x, int y, int minLenght = 3)
        {
            List<BaseGamePiece> leftMatches = boardComponent.matchFinder.FindMatches(x, y, new Vector2(-1, 0), minLenght);
            List<BaseGamePiece> downwardMatches = boardComponent.matchFinder.FindMatches(x, y, new Vector2(0, -1), minLenght);

            leftMatches = boardComponent.ListCheck(leftMatches, ref downwardMatches);

            return (leftMatches.Count > 0 || downwardMatches.Count > 0);
        }
        
        private void CreateGamePiece( BaseGamePiece prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                prefab.SetBoard(boardComponent);
                boardComponent.PlaceGamePiece(prefab, x, y);

                if (falseYOffset != 0)
                {
                    prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                    prefab.MoveGamePiece(x, y, 0.1f);
                }

                prefab.transform.parent = boardComponent.transform;
                prefab.GetComponent<BaseGamePiece>();
            }
        }



        private void BreakTileAt(int x , int y)
        {
            TileComponent tileComponentToBreak = boardComponent.tileArray[x, y];
            if (tileComponentToBreak != null && tileComponentToBreak.tileType == TileType.Breakable)
            {
                BreakTilePfxEvent?.Invoke(tileComponentToBreak.breakableValue, x, y, 0);
                tileComponentToBreak.BreakTile();
            }
        }

        private void BreakTileAt(List<BaseGamePiece> gamePieces)
        {
            foreach (BaseGamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    BreakTileAt(piece.xIndex,piece.yIndex);
                }
            }
        }
        
        public void ClickTile(TileComponent tileComponent)
        {
            if(boardComponent.clickedTileComponent == null && canGetInput)
                boardComponent.clickedTileComponent = tileComponent;
        }

        public void DragToTile(TileComponent tileComponent)
        {
            if (boardComponent.clickedTileComponent != null &&  boardComponent.IsNextTo(tileComponent, boardComponent.clickedTileComponent) && canGetInput) 
                boardComponent.targetTileComponent = tileComponent;
        }
        
        public void ReleaseTile()
        {

            if (boardComponent.clickedTileComponent != null && boardComponent.targetTileComponent != null  && canGetInput )
            {
                StartCoroutine(SwitchTiles(boardComponent.clickedTileComponent,boardComponent.targetTileComponent, () => canGetInput = true));
            }
            else
            {
                boardComponent.clickedTileComponent = null;
                boardComponent.targetTileComponent = null;
            }

            IEnumerator SwitchTiles(TileComponent current , TileComponent target, Action onComplete)
            {

                canGetInput = false;

                BaseGamePiece clickedPiece = boardComponent.gamePieceArray[current.XIndex, current.YIndex];
                BaseGamePiece targetPiece = boardComponent.gamePieceArray[target.XIndex, target.YIndex];


                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.MoveGamePiece(boardComponent.targetTileComponent.XIndex,boardComponent.targetTileComponent.YIndex,_swapTime); 
                    targetPiece.MoveGamePiece(boardComponent.clickedTileComponent.XIndex,boardComponent.clickedTileComponent.YIndex,_swapTime);
                    
                    yield return _swapWaiter;

                    List<BaseGamePiece> clickedPieceMatches = CombineMatches(boardComponent.clickedTileComponent.XIndex, boardComponent.clickedTileComponent.YIndex);
                    List<BaseGamePiece> targetPieceMatches = CombineMatches(boardComponent.targetTileComponent.XIndex, boardComponent.targetTileComponent.YIndex);
                    
                    var colorMatches = boardComponent.GetSameColorPieces(clickedPiece, targetPiece);
                    

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                    {
                        clickedPiece.MoveGamePiece(boardComponent.clickedTileComponent.XIndex,boardComponent.clickedTileComponent.YIndex,_swapTime);
                        targetPiece.MoveGamePiece(boardComponent.targetTileComponent.XIndex,boardComponent.targetTileComponent.YIndex,_swapTime);
                    }
                    else
                    {
                        yield return _swapWaiter;

                        if (boardComponent.levelData.dropBombAfterMatch)
                        {
                            PerformDropBomb(clickedPieceMatches, targetPieceMatches);
                        }

                        if (canDropTiles)
                        {
                            StartCoroutine(ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList()));
                        }
                    }

                    
                    boardComponent.clickedTileComponent = null;
                    boardComponent.targetTileComponent = null;
                }


                onComplete?.Invoke();
                
            }
        }
        

        private List<BaseGamePiece> CombineMatches(int x, int y)
        {
              List<BaseGamePiece> horMatches = boardComponent.FindHorizontalMatches(x, y);
              List<BaseGamePiece> verMatches = boardComponent.FindVerticalMatches(x, y);
            
              horMatches ??= new List<BaseGamePiece>();

              verMatches ??= new List<BaseGamePiece>();

              horMatches = boardComponent.ListCheck(horMatches, ref verMatches);

              var combMatches = horMatches.Union(verMatches).ToList();
              return combMatches;
          }
        
          List<BaseGamePiece> CombineMatches(List<BaseGamePiece> gamePieces)
          {
              List<BaseGamePiece> matches = new List<BaseGamePiece>();

              foreach (BaseGamePiece piece in gamePieces)
              {
                  if (piece == null) continue;
                  matches = matches.Union(CombineMatches(piece.xIndex, piece.yIndex)).ToList();
              }

              return matches;
          }
          

        IEnumerator ClearAndRefillBoard(List<BaseGamePiece> gamePieces)
        {

            yield return _collapseWaiter;
            
            while (true)
            {
                canGetInput = false;

                List<BaseGamePiece> bombedPieces = boardComponent.GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();

                // Chaining bombs 
                bombedPieces = boardComponent.GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();

                List<BaseGamePiece> collectedPieces = FindCollectiblesAt(0, true);
                List<BaseGamePiece> allCollectibles = FindAllCollectibles();
                List<BaseGamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
                collectedPieces = collectedPieces.Union(blockers).ToList();
                boardComponent.levelData.collectibleCount -= collectedPieces.Count;

                gamePieces = gamePieces.Union(collectedPieces).ToList();

                ClearPieceAt(gamePieces, bombedPieces);
                BreakTileAt(gamePieces);
                
                yield return _collapseWaiter;
                
                ActivateDroppedBomb();

                List<BaseGamePiece> movingPieces = boardComponent.CollapseColumnByPieces(gamePieces);

                yield return _collapseWaiter;
                List<BaseGamePiece> matches = CombineMatches(movingPieces);
                
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
            canGetInput = true;
        }
        
        
        private Bomb CreateBomb(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                Bomb bomb = Instantiate(prefab.GetComponent<Bomb>(), new Vector3(x, y, 0), Quaternion.identity);
                bomb.SetBoard(boardComponent);
                bomb.SetCoord(x,y);
                bomb.transform.parent = boardComponent.transform;
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

                if (ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
                {
                   boardComponent.gamePieceArray[x, y] = _clickedTileBomb.GetComponent<BaseGamePiece>();
                }
                _clickedTileBomb = null;
            }

            if (_targetTileBomb != null)
            {
                int x = (int)_targetTileBomb.transform.position.x;
                int y = (int)_targetTileBomb.transform.position.y;

                if (ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
                {
                    boardComponent.gamePieceArray[x, y] = _targetTileBomb.GetComponent<BaseGamePiece>();
                }
                _targetTileBomb = null;
            }
        }

        
        private void PerformDropBomb(List<BaseGamePiece> clickedPieceMatches, List<BaseGamePiece> targetPieceMatches)
        {
            Vector2 swipeDirection =
                new Vector2(boardComponent.targetTileComponent.XIndex - boardComponent.clickedTileComponent.XIndex, boardComponent.targetTileComponent.YIndex - boardComponent.clickedTileComponent.YIndex);
            _clickedTileBomb = DropBomb(boardComponent.clickedTileComponent.XIndex, boardComponent.clickedTileComponent.YIndex, swipeDirection, clickedPieceMatches);
            _targetTileBomb = DropBomb(boardComponent.targetTileComponent.XIndex, boardComponent.targetTileComponent.YIndex, swipeDirection, targetPieceMatches);
        }
        
        Bomb DropBomb(int x, int y, Vector2 swapDirection, List<BaseGamePiece> gamePieces)
        {
            Bomb bomb = null;

            if (gamePieces.Count >= boardComponent.levelData.matchCountForBombDrop)
            {
                if (boardComponent.IsCornerMatch(gamePieces))
                {
                    if (adjacentBombPrefab != null)
                    {
                        bomb = CreateBomb(adjacentBombPrefab, x, y);
                    }
                }
                else
                {
                    if (gamePieces.Count >= boardComponent.levelData.matchCountForColorBombDrop)
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
        
        private List<BaseGamePiece> RemoveCollectibles(List<BaseGamePiece> bombedPieces)
        {
            List<BaseGamePiece> collectiblePieces = FindAllCollectibles();
            List<BaseGamePiece> piecesToRemove = new List<BaseGamePiece>();

            foreach (BaseGamePiece piece in collectiblePieces)
            {
                CollectibleComponent collectibleComponent = piece.GetComponent<CollectibleComponent>();

                if (collectibleComponent != null)
                {
                    if (!collectibleComponent.clearedByBomb)
                    {
                        piecesToRemove.Add(piece);
                    }
                }
            }

            return bombedPieces.Except(piecesToRemove).ToList();

        }
        
        private List<BaseGamePiece> FindAllCollectibles()
        {
            List<BaseGamePiece> foundCollectibles = new List<BaseGamePiece>();

            for (int i = 0; i < boardComponent.height; i++)
            {
                List<BaseGamePiece> collectibleRow = FindCollectiblesAt(i);
                foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
            }

            return foundCollectibles;
        }
        

        private List<BaseGamePiece> FindCollectiblesAt(int row, bool collectedAtBottom = false)
        {
            List<BaseGamePiece> foundCollectibles = new List<BaseGamePiece>();

            for (int i = 0; i < boardComponent.width; i++)
            {
                if (boardComponent.gamePieceArray[i, row] != null)
                {
                    CollectibleComponent collectibleComponent = boardComponent.gamePieceArray[i, row].GetComponent<CollectibleComponent>();

                    if (collectibleComponent != null)
                    {
                        if (!collectedAtBottom || (collectedAtBottom && collectibleComponent.clearedAtBottom))
                        {
                            foundCollectibles.Add(boardComponent.gamePieceArray[i,row]);
                        }
                    }
                }
            }

            return foundCollectibles;
        }

        bool CanAddCollectible()
        {
            return (Random.Range(0f, 1f) <= boardComponent.levelData.chanceForCollectible && collectiblePrefabs.Length > 0 &&
                    boardComponent.levelData.collectibleCount < boardComponent.levelData.maxCollectibleCount);
        }


    }
}