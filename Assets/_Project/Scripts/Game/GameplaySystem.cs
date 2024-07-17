

using _Project.Scripts.Audio;
using _Project.Scripts.Game.Board;
using _Project.Scripts.Game.Particles;
using _Project.Scripts.Game.Player;

namespace _Project.Scripts.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        private BoardManager _boardManager;
        private InputManager _inputManager;
        private ScoreController _scoreController;
        private SoundManager _soundManager;
        
        protected override void SetupManagers()
        {
            _boardManager = GetManager<BoardManager>();
            _inputManager = GetManager<InputManager>();
            _particleManager = GetManager<ParticleManager>();
            _soundManager = GetManager<SoundManager>();
            _scoreController = new ScoreController();

            _boardManager.Setup();
            _inputManager.Setup();
            _particleManager.Setup();
            _soundManager.Setup();
        }

        public override void Setup()
        {
            base.Setup();
            SetupEvents();
            StartGame();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if (_boardManager.canGetInput == true)
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
            _boardManager.AddScoreEvent += _scoreController.AddScore;
            _boardManager.GameStatusEvent += ControlGameStatus;
            _boardManager.PlayPopSoundEvent += _soundManager.PlayPopFX;
            _boardManager.PlayBombSoundEvent += _soundManager.PlayBombFX;
            
        }

        private void StartGame()
        {
            _boardManager.SetGameBoard();
            _soundManager.Play("Main",true);
        }

        private void ControlGameStatus()
        {

            if (_boardManager.boardComponent.movesLeft > 0 && _scoreController.GetCurrentScore() >= _boardManager.boardComponent.targetScoreToWin)
            {
                _boardManager.CloseGameBoard();
                _boardManager.canGetInput = false;
                _boardManager.canDropTiles = false;
                _soundManager.StopAll();
                _soundManager.Play("Win",false);
            }
            if (_boardManager.boardComponent.movesLeft == 0)
            {                
                _boardManager.CloseGameBoard();
                _boardManager.canGetInput = false;
                _boardManager.canDropTiles = false;
                _soundManager.StopAll();
            }
            

        }
        
        
    }
}
