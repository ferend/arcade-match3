using _Project.Scripts.Utility;

namespace _Project.Scripts.Game.Gamepiece
{
    public class CollectibleComponent : BaseGamePiece
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
