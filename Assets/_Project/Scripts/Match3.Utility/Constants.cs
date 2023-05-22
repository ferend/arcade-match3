using UnityEngine;

namespace _Project.Scripts.Match3.Utility
{
    public static class Constants
    {
        public const int BOARD_WIDTH = 7;
        public const int BOARD_HEIGHT = 9;
        
        public const int BORDER_SIZE = 2;

        public static readonly Color[] TILE_COLORS = new[]
        {
            Color.blue,
            Color.red,
            Color.yellow,
            Color.green, 
            Color.magenta
        };

        #region UI
        public const float defaultTransitionDuration = 0.25f;
        public const float overlayTransitionDuration = 0.5f;
        public const float splashScreenDuration = 2.0f;
        public const float popupOpenDuration = 0.5f;
        public const float popupUnderlayTransitionDuration = 0.5f;
        public const float popupCloseDuration = 0.5f;
        #endregion
    }
}

