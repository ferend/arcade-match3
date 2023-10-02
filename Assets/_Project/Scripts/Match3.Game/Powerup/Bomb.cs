using System.Collections.Generic;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Powerup
{
    public class Bomb : GamePiece
    {
        public BombType bombType;
        
        private void Start()
        {
            if (bombType == BombType.Color)
            {
                SetDefaultSpriteColor();
            }

            scoreValue = 2;
        }

        public List<GamePiece> GetRowPieces(int row)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < gameBoard.width; i++)
            {
                if (gameBoard.gamePieceArray[i, row] != null)
                {
                    gamePieces.Add(gameBoard.gamePieceArray[i,row]);
                }
            }

            return gamePieces;
        }

        public List<GamePiece> GetColumnPieces(int column)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < gameBoard.height; i++)
            {
                if (gameBoard.gamePieceArray[column, i] != null)
                {
                    gamePieces.Add(gameBoard.gamePieceArray[column, i]);
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
                    if (ExtensionMethods.IsInBounds(i, j, gameBoard.width, gameBoard.height))
                    {
                        gamePieces.Add(gameBoard.gamePieceArray[i,j]);
                    }
                }
            }

            return gamePieces;
        }
        


    }
}
