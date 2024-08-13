using _Project.Script.UI;
using _Project.Scripts.Audio;
using _Project.Scripts.Core;
using _Project.Scripts.Game.Board;
using _Project.Scripts.Game.Particles;
using _Project.Scripts.Game.Player;
using _Project.Scripts.Game.UI;

namespace _Project.Scripts.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        private BoardManager _boardManager;
        private InputManager _inputManager;
        
        protected override void SetupManagers()
        {
            _boardManager = GetManager<BoardManager>();
            _inputManager = GetManager<InputManager>();
            _particleManager = GetManager<ParticleManager>();

            _inputManager.Setup();
            _particleManager.Setup();
            _boardManager.Setup();
            
            SetupEvents();
        }
        

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if (_boardManager != null && _boardManager.canGetInput == true)
            {
                _inputManager.Tick(deltaTime);    
            }
            
        }
        
        public void OpenHUD()
        {
            var uiManager = ServiceLocator.GetService<UISystem>();
            uiManager.Show<MainMenu>();
        }

        private void SetupEvents()
        {
            _inputManager.ClickTileEvent += _boardManager.ClickTile;
            _inputManager.DragTileEvent += _boardManager.DragToTile;
            _inputManager.ReleaseTileEvent += _boardManager.ReleaseTile;
            
            _boardManager.ClearPiecePfxEvent += _particleManager.ClearPiecePfxAt;
            _boardManager.BreakTilePfxEvent += _particleManager.BreakTilePfxAt;
            _boardManager.BombPiecePfxEvent += _particleManager.BombPfxAt;
            
        }
        
        private void OnDestroy()
        {
            ServiceLocator.DisposeService<UISystem>();
        }
        
    }
}
