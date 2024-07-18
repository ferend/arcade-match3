using System.Collections.Generic;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Utility;
using UnityEngine;

namespace _Project.Scripts.Game.Board
{
    public class MatchAction
    {
        private readonly int width;
        private readonly int height;
        private readonly BaseGamePiece[,] gamePieceArray;

        public MatchAction(int width, int height, BaseGamePiece[,] gamePieceArray)
        {
            this.width = width;
            this.height = height;
            this.gamePieceArray = gamePieceArray;
        }

        public List<BaseGamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
        {
            List<BaseGamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
            List<BaseGamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

            rightMatches ??= new List<BaseGamePiece>();
            leftMatches ??= new List<BaseGamePiece>();

            foreach (BaseGamePiece piece in leftMatches)
            {
                if (!rightMatches.Contains(piece))
                {
                    rightMatches.Add(piece);
                }
            }

            return (rightMatches.Count >= minLenght) ? rightMatches : null;
        }

        public List<BaseGamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
        {
            List<BaseGamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
            List<BaseGamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

            upwardMatches ??= new List<BaseGamePiece>();
            downwardMatches ??= new List<BaseGamePiece>();

            foreach (BaseGamePiece piece in downwardMatches)
            {
                if (!upwardMatches.Contains(piece))
                {
                    upwardMatches.Add(piece);
                }
            }

            return (upwardMatches.Count >= minLenght) ? upwardMatches : null;
        }

        public List<BaseGamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
        {
            List<BaseGamePiece> matches = new List<BaseGamePiece>();

            BaseGamePiece startPiece = null;

            if (ExtensionMethods.IsInBounds(startX, startY, width, height))
            {
                startPiece = gamePieceArray[startX, startY];
            }

            if (startPiece != null)
            {
                matches.Add(startPiece);
            }
            else
            {
                return null;
            }

            int nextX;
            int nextY;

            int maxValue = (width > height) ? width : height;

            for (int i = 1; i < maxValue - 1; i++)
            {
                nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
                nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

                if (!ExtensionMethods.IsInBounds(nextX, nextY, width, height))
                {
                    break;
                }

                BaseGamePiece nextPiece = gamePieceArray[nextX, nextY];

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

            return (matches.Count >= minLength) ? matches : null;
        }
        

    }
}
