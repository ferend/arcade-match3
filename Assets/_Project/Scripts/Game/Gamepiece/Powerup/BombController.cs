using _Project.Scripts.Game.Gamepiece;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Game.Board
{


    namespace _Project.Scripts.Game.Board
    {
        public class BombController
        {
            private readonly BaseGamePiece[,] gamePieceArray;

            public BombController(BaseGamePiece[,] gamePieceArray, BoardComponent.RemoveCollectibleDelegate removeCollectibleDelegate)
            {
                this.gamePieceArray = gamePieceArray;
            }

            public List<BaseGamePiece> GetBombedPieces(List<BaseGamePiece> gamePieces)
            {
                List<BaseGamePiece> allPiecesToClear = new List<BaseGamePiece>();

                foreach (BaseGamePiece piece in gamePieces)
                {
                    if (piece != null)
                    {
                        List<BaseGamePiece> piecesToClear = new List<BaseGamePiece>();
                        Bomb bomb = piece.GetComponent<Bomb>();
                        if (bomb != null)
                        {
                            piecesToClear = bomb.bombType switch
                            {
                                BombType.Column => bomb.GetColumnPieces(bomb.xIndex),
                                BombType.Row => bomb.GetRowPieces(bomb.yIndex),
                                BombType.Adjacent => bomb.GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1),
                                _ => piecesToClear
                            };
                        }

                        allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                    }
                }

                return allPiecesToClear;
            }
        }
    }

}