using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.Tile;
using _Project.Scripts.Game.TileActor;

namespace _Project.Scripts.Game.Board
{
    using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Game.Board
{
    public class CollapseAction
    {
        private readonly int width;
        private readonly int height;
        private readonly BaseGamePiece[,] gamePieceArray;
        private readonly TileComponent[,] tileArray;

        public CollapseAction(int width, int height, BaseGamePiece[,] gamePieceArray, TileComponent[,] tileArray)
        {
            this.width = width;
            this.height = height;
            this.gamePieceArray = gamePieceArray;
            this.tileArray = tileArray;
        }

        public List<BaseGamePiece> CollapseColumnByPieces(List<BaseGamePiece> gamePieces)
        {
            List<BaseGamePiece> movingPieces = new List<BaseGamePiece>();
            List<int> columnsToCollapse = GetColumns(gamePieces);

            foreach (int column in columnsToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
            }

            return movingPieces;
        }

        private List<int> GetColumns(List<BaseGamePiece> gamePieces)
        {
            List<int> columns = new List<int>();

            foreach (BaseGamePiece piece in gamePieces)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }

            return columns;
        }

        private List<BaseGamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<BaseGamePiece> movingPieces = new List<BaseGamePiece>();

            for (int i = 0; i < height - 1; i++)
            {
                if (gamePieceArray[column, i] == null && tileArray[column, i].tileType != TileType.Obstacle)
                {
                    for (int j = i + 1; j < height; j++)
                    {
                        if (gamePieceArray[column, j] != null)
                        {
                            gamePieceArray[column, j].MoveGamePiece(column, i, collapseTime * (j - i));
                            gamePieceArray[column, i] = gamePieceArray[column, j];
                            gamePieceArray[column, i].SetCoord(column, i);

                            if (!movingPieces.Contains(gamePieceArray[column, i]))
                            {
                                movingPieces.Add(gamePieceArray[column, i]);
                            }

                            gamePieceArray[column, j] = null;
                            break;
                        }
                    }
                }
            }
            return movingPieces;
        }
    }
}

}