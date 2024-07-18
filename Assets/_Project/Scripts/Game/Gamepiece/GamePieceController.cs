using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.Tile;
using _Project.Scripts.Game.TileActor;
using _Project.Scripts.Utility;

namespace _Project.Scripts.Game.Board
{
    public class GamePieceController
    {
        private readonly int width;
        private readonly int height;
        private readonly BaseGamePiece[,] gamePieceArray;

        public GamePieceController(int width, int height, BaseGamePiece[,] gamePieceArray)
        {
            this.width = width;
            this.height = height;
            this.gamePieceArray = gamePieceArray;
        }

        public void PlaceGamePiece(BaseGamePiece baseGamePiece, int x, int y)
        {
            if (baseGamePiece == null) return;

            baseGamePiece.transform.position = new Vector3(x, y, 0);
            baseGamePiece.transform.rotation = Quaternion.identity;

            if (ExtensionMethods.IsInBounds(x, y, width, height))
            {
                gamePieceArray[x, y] = baseGamePiece;
            }

            baseGamePiece.SetCoord(x, y);
        }

        public List<BaseGamePiece> GetSameColorPieces(BaseGamePiece clickedPiece, BaseGamePiece targetPiece)
        {
            List<BaseGamePiece> colorMatches = new List<BaseGamePiece>();

            if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
            {
                clickedPiece.gamePieceColor = targetPiece.gamePieceColor;
                colorMatches = FindAllMatchValues(clickedPiece.gamePieceColor);
            }
            else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
            {
                targetPiece.gamePieceColor = clickedPiece.gamePieceColor;
                colorMatches = FindAllMatchValues(targetPiece.gamePieceColor);
            }
            else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
            {
                foreach (BaseGamePiece piece in gamePieceArray)
                {
                    if (!colorMatches.Contains(piece))
                    {
                        colorMatches.Add(piece);
                    }
                }
            }

            return colorMatches;
        }

        public bool IsNextTo(TileComponent start, TileComponent end)
        {
            if (Mathf.Abs(start.XIndex - end.XIndex) == 1 && start.YIndex == end.YIndex)
            {
                return true;
            }

            return Mathf.Abs(start.YIndex - end.YIndex) == 1 && start.XIndex == end.XIndex;
        }

        public bool IsCornerMatch(List<BaseGamePiece> gamePieces)
        {
            bool vertical = false;
            bool horizontal = false;
            int xStart = -1;
            int yStart = -1;

            foreach (BaseGamePiece piece in gamePieces)
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

        private List<BaseGamePiece> FindAllMatchValues(Color matchValue)
        {
            List<BaseGamePiece> foundPieces = new List<BaseGamePiece>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (gamePieceArray[i, j] != null)
                    {
                        if (gamePieceArray[i, j].gamePieceColor == matchValue)
                        {
                            foundPieces.Add(gamePieceArray[i, j]);
                        }
                    }
                }
            }
            return foundPieces;
        }

        private bool IsColorBomb(BaseGamePiece baseGamePiece)
        {
            Bomb bomb = baseGamePiece.GetComponent<Bomb>();

            return bomb != null && bomb.bombType == BombType.Color;
        }

         public void SetupGamePieces(BoardComponent boardComponent)
        {
            foreach (StartingTile sPiece in boardComponent.levelData.startingGamePieces)
            {
                if (sPiece != null)
                {
                    BaseGamePiece piece = Object.Instantiate(sPiece.tilePrefab, new Vector3(sPiece.x, sPiece.y, 0),
                        Quaternion.identity).GetComponent<BaseGamePiece>();
                    CreateGamePiece(boardComponent, piece, sPiece.x, sPiece.y, 10, 0.1f);
                }
            }
        }

        public void ClearPieceAtPosition(BoardComponent boardComponent, int x, int y)
        {
            BaseGamePiece pieceToClear = boardComponent.gamePieceArray[x, y];
            if (pieceToClear != null)
            {
                boardComponent.gamePieceArray[x, y] = null;
                Object.Destroy(pieceToClear.gameObject);
            }
        }

        public void CreateGamePiece(BoardComponent boardComponent, BaseGamePiece prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && ExtensionMethods.IsInBounds(x, y, boardComponent.width, boardComponent.height))
            {
                prefab.SetBoard(boardComponent);
                boardComponent.PlaceGamePiece(prefab, x, y);
                if (falseYOffset != 0)
                {
                    prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                    prefab.MoveGamePiece(x, y, moveTime);
                }
                prefab.transform.parent = boardComponent.transform;
                prefab.GetComponent<BaseGamePiece>();
            }
        }

        public List<BaseGamePiece> FindAllCollectibles(BoardComponent boardComponent)
        {
            List<BaseGamePiece> foundCollectibles = new List<BaseGamePiece>();
            for (int i = 0; i < boardComponent.height; i++)
            {
                List<BaseGamePiece> collectibleRow = FindCollectiblesAt(boardComponent, i);
                foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
            }
            return foundCollectibles;
        }

        public List<BaseGamePiece> FindCollectiblesAt(BoardComponent boardComponent, int row, bool collectedAtBottom = false)
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
                            foundCollectibles.Add(boardComponent.gamePieceArray[i, row]);
                        }
                    }
                }
            }
            return foundCollectibles;
        }

        public List<BaseGamePiece> RemoveCollectibles(BoardComponent boardComponent, List<BaseGamePiece> bombedPieces)
        {
            List<BaseGamePiece> collectiblePieces = FindAllCollectibles(boardComponent);
            List<BaseGamePiece> intersectingPieces = bombedPieces.Intersect(collectiblePieces).ToList();
            if (intersectingPieces.Count > 0)
            {
                foreach (BaseGamePiece piece in intersectingPieces)
                {
                    if (piece != null)
                    {
                        Object.Destroy(piece.gameObject);
                    }
                }
                return intersectingPieces;
            }
            return null;
        }
        
    }
}

