using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.TileActor;
using _Project.Scripts.Utility;

namespace _Project.Scripts.Game.Board
{
    public class GamePieceManager
    {
        private readonly int width;
        private readonly int height;
        private readonly BaseGamePiece[,] gamePieceArray;

        public GamePieceManager(int width, int height, BaseGamePiece[,] gamePieceArray)
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

        
    }
}

