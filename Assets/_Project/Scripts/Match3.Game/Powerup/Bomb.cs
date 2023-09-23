using System.Collections.Generic;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Utility;

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

        public List<GamePiece> GetRowPieces(int row)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < _gameBoard.Width; i++)
            {
                if (_gameBoard.GamePieceArray[i, row] != null)
                {
                    gamePieces.Add(_gameBoard.GamePieceArray[i,row]);
                }
            }

            return gamePieces;
        }

        public List<GamePiece> GetColumnPieces(int column)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for (int i = 0; i < _gameBoard.Height; i++)
            {
                if (_gameBoard.GamePieceArray[column, i] != null)
                {
                    gamePieces.Add(_gameBoard.GamePieceArray[column, i]);
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
                    if (ExtensionMethods.IsInBounds(i, j, _gameBoard.Width, _gameBoard.Height))
                    {
                        gamePieces.Add(_gameBoard.GamePieceArray[i,j]);
                    }
                }
            }

            return gamePieces;
        }
    }
}
