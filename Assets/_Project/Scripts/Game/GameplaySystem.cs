

using _Project.Scripts.Audio;
using _Project.Scripts.Game.Board;
using _Project.Scripts.Game.Particles;
using _Project.Scripts.Game.Player;
using Unity.VisualScripting;

namespace _Project.Scripts.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        private BoardManager _boardManager;
        private InputManager _inputManager;
        private SoundManager _soundManager;
        
        protected override void SetupManagers()
        {
            _boardManager = GetManager<BoardManager>();
            _inputManager = GetManager<InputManager>();
            _particleManager = GetManager<ParticleManager>();
            _soundManager = GetManager<SoundManager>();

            _inputManager.Setup();
            _particleManager.Setup();
            _soundManager.Setup();
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

        private void SetupEvents()
        {
            _inputManager.ClickTileEvent += _boardManager.ClickTile;
            _inputManager.DragTileEvent += _boardManager.DragToTile;
            _inputManager.ReleaseTileEvent += _boardManager.ReleaseTile;
            
            _boardManager.ClearPiecePfxEvent += _particleManager.ClearPiecePfxAt;
            _boardManager.BreakTilePfxEvent += _particleManager.BreakTilePfxAt;
            _boardManager.BombPiecePfxEvent += _particleManager.BombPfxAt;
            _boardManager.PlayPopSoundEvent += _soundManager.PlayPopFX;
            _boardManager.PlayBombSoundEvent += _soundManager.PlayBombFX;
            
        }
        
    }
}
