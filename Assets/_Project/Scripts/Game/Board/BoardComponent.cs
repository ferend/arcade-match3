using System;
using System.Collections.Generic;
using _Project.Scripts.Game.Board._Project.Scripts.Game.Board;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.Level;
using _Project.Scripts.Game.Tile;
using _Project.Scripts.Game.TileActor;
using _Project.Scripts.Utility;
using UnityEngine;

namespace _Project.Scripts.Game.Board
{
    public class BoardComponent : MonoBehaviour
    {
        [SerializeField] internal LevelData levelData;

        internal readonly int width = Constants.BOARD_WIDTH;
        internal readonly int height = Constants.BOARD_HEIGHT;

        internal TileComponent[,] tileArray;
        internal BaseGamePiece[,] gamePieceArray;
        internal TileComponent clickedTileComponent;
        internal TileComponent targetTileComponent;


        internal GamePieceController gamePieceController;
        internal MatchAction matchAction;
        private CollapseAction _collapseAction;
        private BombController _bombController;
        
        public void SetupBoardComponent()
        {
            InitTileArray();
            InitGamePieceArray();
            matchAction = new MatchAction(width, height, gamePieceArray);
            gamePieceController = new GamePieceController(width, height, gamePieceArray);
            _collapseAction = new CollapseAction(width, height, gamePieceArray, tileArray);
            _bombController = new BombController(gamePieceArray);
        }
        
        private void InitTileArray() => tileArray = new TileComponent[width, height];
        private void InitGamePieceArray() => gamePieceArray = new BaseGamePiece[width, height];


        public void PlaceGamePiece(BaseGamePiece baseGamePiece, int x, int y)
        {
            if (gamePieceController != null) {
                gamePieceController.PlaceGamePiece(baseGamePiece, x, y);
            }
        }

        public List<BaseGamePiece> GetSameColorPieces(BaseGamePiece clickedPiece, BaseGamePiece targetPiece)
        {
            return gamePieceController.GetSameColorPieces(clickedPiece, targetPiece);
        }

        public bool IsNextTo(TileComponent start, TileComponent end)
        {
            return gamePieceController.IsNextTo(start, end);
        }

        public List<BaseGamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
        {
            return matchAction.FindHorizontalMatches(startX, startY, minLenght);
        }

        public List<BaseGamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
        {
            return matchAction.FindVerticalMatches(startX, startY, minLenght);
        }

        public List<BaseGamePiece> CollapseColumnByPieces(List<BaseGamePiece> gamePieces)
        {
            return _collapseAction.CollapseColumnByPieces(gamePieces);
        }

        public List<BaseGamePiece> GetBombedPieces(List<BaseGamePiece> gamePieces)
        {
            return _bombController.GetBombedPieces(gamePieces);
        }

        public bool IsCornerMatch(List<BaseGamePiece> gamePieces)
        {
            return gamePieceController.IsCornerMatch(gamePieces);
        }
        public List<BaseGamePiece> ListCheck(List<BaseGamePiece> leftMatches, ref List<BaseGamePiece> downwardMatches)
        {
            
            if (leftMatches == null)
            {
                leftMatches = new List<BaseGamePiece>();
            }

            if (downwardMatches == null)
            {
                downwardMatches = new List<BaseGamePiece>();
            }

            return leftMatches;
        }
    }
}
