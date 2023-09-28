using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Match3.Actor;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Game.Powerup;
using _Project.Scripts.Match3.Game.TileActor;
using _Project.Scripts.Match3.Utility;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.BoardActor
{
    
    public class Board : MonoBehaviour
    {
        [Header("Rules")]
        [SerializeField] internal bool dropBombAfterMatch = false;
        [SerializeField] internal int matchCountForBombDrop = 4;
        [SerializeField] internal int matchCountForColorBombDrop = 5;
        [SerializeField] internal int maxCollectibleCount = 3;
        [SerializeField] internal int collectibleCount = 0;
        [Range(0,1)]
        [SerializeField] internal float chanceForCollectible = 0.1f;
        
        internal readonly int width = Constants.BOARD_WIDTH;
        internal readonly int height = Constants.BOARD_HEIGHT;

        [Space(10)]
        [Header("Pre-Defined Pieces")]
        [SerializeField] internal StartingTile[] startingTiles;
        [SerializeField] internal StartingTile[] startingGamePieces;
        
        internal Tile[,] tileArray;
        internal GamePiece[,] gamePieceArray;

        internal Tile clickedTile;
        internal Tile targetTile;

        internal delegate List<GamePiece> RemoveCollectibleDelegate(List<GamePiece> bombedPieces);

        internal RemoveCollectibleDelegate removeCollectibleDelegate;

        public void PlaceGamePiece(GamePiece gamePiece, int x , int y )
        {
            if(gamePiece == null) return;
            
            gamePiece.transform.position = new Vector3(x, y, 0);
            gamePiece.transform.rotation = Quaternion.identity;
            
            if (ExtensionMethods.IsInBounds(x, y, width, height))
            {            
                gamePieceArray[x, y] = gamePiece;
            }
            
            gamePiece.SetCoord(x,y);
        }
        
        
        public List<GamePiece> GetSameColorPieces(GamePiece clickedPiece, GamePiece targetPiece)
        {
            List<GamePiece> colorMatches = new List<GamePiece>();

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
                foreach (GamePiece piece in gamePieceArray)
                {
                    if (!colorMatches.Contains(piece))
                    {
                        colorMatches.Add(piece);
                    }
                }
            }

            return colorMatches;
        }

        

        public bool IsNextTo(Tile start , Tile end)
        {
            if (Mathf.Abs(start.XIndex - end.XIndex) == 1 && start.YIndex == end.YIndex)
            {
                return true;
            }

            return Mathf.Abs(start.YIndex - end.YIndex) == 1 && start.XIndex == end.XIndex;
        }

        

        public List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
        {
            List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0),2);
            List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0),2);

            rightMatches ??= new List<GamePiece>();
            leftMatches ??= new List<GamePiece>();
            
            foreach (GamePiece piece in leftMatches)
            {
                if (!rightMatches.Contains(piece))
                {
                    rightMatches.Add(piece);
                }
            }
            
            return (rightMatches.Count >= minLenght) ? rightMatches : null ;
        }
        

        public List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
        {
            List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1),2);
            List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1),2);

            upwardMatches ??= new List<GamePiece>();
            downwardMatches ??= new List<GamePiece>();
            
            foreach (GamePiece piece in downwardMatches)
            {
                if (!upwardMatches.Contains(piece))
                {
                    upwardMatches.Add(piece);
                }
            }
            
            return (upwardMatches.Count >= minLenght) ? upwardMatches : null ;
        }
        

        public List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            GamePiece startPiece = null;

            if (ExtensionMethods.IsInBounds(startX, startY,width,height))
            {
                startPiece = gamePieceArray[startX, startY];
            }

            if (startPiece !=null)
            {
                matches.Add(startPiece);
            }

            else
            {
                return null;
            }

            int nextX;
            int nextY;

            int maxValue = (width > height) ? width: height;

            for (int i = 1; i < maxValue - 1; i++)
            {
                nextX = startX + (int) Mathf.Clamp(searchDirection.x,-1,1) * i;
                nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

                if (!ExtensionMethods.IsInBounds(nextX, nextY,width,height))
                {
                    break;
                }

                GamePiece nextPiece = gamePieceArray[nextX, nextY];

                if (nextPiece == null)
                {
                    break;
                }

                if (nextPiece.gamePieceColor == startPiece.gamePieceColor && !matches.Contains(nextPiece) && nextPiece.gamePieceColor != default)
                {
                    matches.Add(nextPiece);
                }

                else
                {
                    break;
                }

            }

            if (matches.Count >= minLength)
            {
                return matches;
            }
            return null;
        }
        
        public List<GamePiece> ListCheck(List<GamePiece> leftMatches, ref List<GamePiece> downwardMatches)
        {
            if (leftMatches == null)
            {
                leftMatches = new List<GamePiece>();
            }

            if (downwardMatches == null)
            {
                downwardMatches = new List<GamePiece>();
            }

            return leftMatches;
        }
        
        List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();

            for (int i = 0; i < height - 1; i++)
            {
                if (gamePieceArray[column, i] == null && tileArray[column,i].tileType != TileType.Obstacle)
                {
                    for (int j = i + 1; j < height; j++)
                    {
                        if (gamePieceArray[column, j] != null)
                        {
                            gamePieceArray[column, j].MoveGamePiece(column, i, collapseTime * (j - i) );
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

        public List<GamePiece> CollapseColumnByPieces(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<int> columnsToCollapse = GetColumns(gamePieces);

            foreach (int column in columnsToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
            }

            return movingPieces;
        }

        List<int> GetColumns(List<GamePiece> gamePieces)
        {
            List<int> columns = new List<int>();

            foreach (GamePiece piece in gamePieces)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }

            return columns;
        }
        
        
        public List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
        {
            List<GamePiece> allPiecesToClear = new List<GamePiece>();

            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    List<GamePiece> piecesToClear = new List<GamePiece>();
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
                    allPiecesToClear = removeCollectibleDelegate(allPiecesToClear);
                }
            }

            return allPiecesToClear;
        }
        

        public bool IsCornerMatch(List<GamePiece> gamePieces)
        {
            bool vertical = false;
            bool horizontal = false;
            int xStart = -1;
            int yStart = -1;

            foreach (GamePiece piece in gamePieces)
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

        private List<GamePiece> FindAllMatchValues(Color matchValue)
        {
            List<GamePiece> foundPieces = new List<GamePiece>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (gamePieceArray[i,j] !=null)
                    {
                        if (gamePieceArray[i,j].gamePieceColor == matchValue)
                        {
                            foundPieces.Add(gamePieceArray[i,j]);
                        }
                    }
                }
            }
            return foundPieces;
        }

        private bool IsColorBomb(GamePiece gamePiece)
        {
            Bomb bomb = gamePiece.GetComponent<Bomb>();
            
            return bomb != null && bomb.bombType == BombType.Color;
        }
        
        
        private void OnDrawGizmos()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Handles.Label(new Vector3(i,j), "x" + i + "y" + j);
                }
                
            }
        }
    }
}
