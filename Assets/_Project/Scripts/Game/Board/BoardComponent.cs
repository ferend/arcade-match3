using System;
using System.Collections.Generic;
using _Project.Scripts.Game.Board._Project.Scripts.Game.Board;
using _Project.Scripts.Game.Gamepiece;
using _Project.Scripts.Game.Tile;
using _Project.Scripts.Game.TileActor;
using _Project.Scripts.Utility;
using UnityEngine;

namespace _Project.Scripts.Game.Board
{
    public class BoardComponent : MonoBehaviour
    {
        [Header("Rules")]
        [SerializeField] internal bool dropBombAfterMatch = false;
        [SerializeField] internal int matchCountForBombDrop = 4;
        [SerializeField] internal int matchCountForColorBombDrop = 5;
        [SerializeField] internal int maxCollectibleCount = 3;
        [SerializeField] internal int collectibleCount = 0;

        [Range(0, 1)]
        [SerializeField] internal float chanceForCollectible = 0.1f;

        internal readonly int width = Constants.BOARD_WIDTH;
        internal readonly int height = Constants.BOARD_HEIGHT;

        [Space(10)]
        [Header("Pre-Defined Pieces")]
        [SerializeField] internal StartingTile[] startingTiles;
        [SerializeField] internal StartingTile[] startingGamePieces;

        internal TileComponent[,] tileArray;
        internal BaseGamePiece[,] gamePieceArray;
        internal TileComponent clickedTileComponent;
        internal TileComponent targetTileComponent;

        public delegate List<BaseGamePiece> RemoveCollectibleDelegate(List<BaseGamePiece> bombedPieces);
        internal RemoveCollectibleDelegate removeCollectibleDelegate;

        private GamePieceManager _gamePieceManager;
        public MatchFinder matchFinder;
        private CollapseController _collapseController;
        private BombController _bombController;
        
        public void SetupBoardComponent()
        {
            InitTileArray();
            InitGamePieceArray();
            matchFinder = new MatchFinder(width, height, gamePieceArray);
            _gamePieceManager = new GamePieceManager(width, height, gamePieceArray);
            _collapseController = new CollapseController(width, height, gamePieceArray, tileArray);
            _bombController = new BombController(gamePieceArray, removeCollectibleDelegate);
        }
        
        private void InitTileArray() => tileArray = new TileComponent[width, height];
        private void InitGamePieceArray() => gamePieceArray = new BaseGamePiece[width, height];


        public void PlaceGamePiece(BaseGamePiece baseGamePiece, int x, int y)
        {
            if (_gamePieceManager != null) {
                _gamePieceManager.PlaceGamePiece(baseGamePiece, x, y);
            }
        }

        public List<BaseGamePiece> GetSameColorPieces(BaseGamePiece clickedPiece, BaseGamePiece targetPiece)
        {
            return _gamePieceManager.GetSameColorPieces(clickedPiece, targetPiece);
        }

        public bool IsNextTo(TileComponent start, TileComponent end)
        {
            return _gamePieceManager.IsNextTo(start, end);
        }

        public List<BaseGamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
        {
            return matchFinder.FindHorizontalMatches(startX, startY, minLenght);
        }

        public List<BaseGamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
        {
            return matchFinder.FindVerticalMatches(startX, startY, minLenght);
        }

        public List<BaseGamePiece> CollapseColumnByPieces(List<BaseGamePiece> gamePieces)
        {
            return _collapseController.CollapseColumnByPieces(gamePieces);
        }

        public List<BaseGamePiece> GetBombedPieces(List<BaseGamePiece> gamePieces)
        {
            return _bombController.GetBombedPieces(gamePieces);
        }

        public bool IsCornerMatch(List<BaseGamePiece> gamePieces)
        {
            return _gamePieceManager.IsCornerMatch(gamePieces);
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
