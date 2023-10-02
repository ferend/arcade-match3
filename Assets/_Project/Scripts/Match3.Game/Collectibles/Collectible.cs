using System;
using _Project.Scripts.Match3.Game.PieceActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Collectibles
{
    public class Collectible : GamePiece
    {
        internal readonly bool clearedByBomb = false;
        internal readonly bool clearedAtBottom = false;
        
        private void Start()
        {
            gamePieceColor = Constants.TILE_COLORS[0];
            SetDefaultSpriteColor();
            scoreValue = 100;
        }
    }
}
