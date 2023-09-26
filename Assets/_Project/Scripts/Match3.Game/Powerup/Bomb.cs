using System.Collections.Generic;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Powerup
{
    public enum BombType
    {
        None,
        Column,
        Row,
        Adjacent,
        Color
    }
    public class Bomb : GamePiece
    {
        public BombType bombType;

        private void Start()
        {
          SetDefaultSpriteColor();
        }

        private void SetDefaultSpriteColor()
        {
            if (bombType == BombType.Color)
            {
                this.SpriteRenderer.color = Color.white;
            }
        }

        public List<GamePiece> GetRowPieces(int row)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < GameBoard.Width; i++)
            {
                if (GameBoard.GamePieceArray[i, row] != null)
                {
                    gamePieces.Add(GameBoard.GamePieceArray[i,row]);
                }
            }

            return gamePieces;
        }

        public List<GamePiece> GetColumnPieces(int column)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < GameBoard.Height; i++)
            {
                if (GameBoard.GamePieceArray[column, i] != null)
                {
                    gamePieces.Add(GameBoard.GamePieceArray[column, i]);
                }
            }

            return gamePieces;
        }

        public List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = x - offset; i <= x + offset; i++)
            {
                for (int j = y - offset; j <= y + offset; j++)
                {
                    if (ExtensionMethods.IsInBounds(i, j, GameBoard.Width, GameBoard.Height))
                    {
                        gamePieces.Add(GameBoard.GamePieceArray[i,j]);
                    }
                }
            }

            return gamePieces;
        }
        


    }
}
