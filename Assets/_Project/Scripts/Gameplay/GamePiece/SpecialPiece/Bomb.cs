using System.Collections.Generic;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Powerup
{
    public class Bomb : BaseGamePiece
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

        public List<BaseGamePiece> GetRowPieces(int row)
        {
            List<BaseGamePiece> gamePieces = new List<BaseGamePiece>();

            for (int i = 0; i < gameBoardComponent.width; i++)
            {
                if (gameBoardComponent.gamePieceArray[i, row] != null)
                {
                    gamePieces.Add(gameBoardComponent.gamePieceArray[i,row]);
                }
            }

            return gamePieces;
        }

        public List<BaseGamePiece> GetColumnPieces(int column)
        {
            List<BaseGamePiece> gamePieces = new List<BaseGamePiece>();

            for (int i = 0; i < gameBoardComponent.height; i++)
            {
                if (gameBoardComponent.gamePieceArray[column, i] != null)
                {
                    gamePieces.Add(gameBoardComponent.gamePieceArray[column, i]);
                }
            }

            return gamePieces;
        }

        public List<BaseGamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
        {
            List<BaseGamePiece> gamePieces = new List<BaseGamePiece>();

            for (int i = x - offset; i <= x + offset; i++)
            {
                for (int j = y - offset; j <= y + offset; j++)
                {
                    if (ExtensionMethods.IsInBounds(i, j, gameBoardComponent.width, gameBoardComponent.height))
                    {
                        gamePieces.Add(gameBoardComponent.gamePieceArray[i,j]);
                    }
                }
            }

            return gamePieces;
        }
        


    }
}
