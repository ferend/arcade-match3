using _Project.Scripts.Match3.Game.BoardActor;
using _Project.Scripts.Match3.Game.Effects;
using _Project.Scripts.Match3.Game.Input;
using _Project.Scripts.Match3.Game.Player;
using _Project.Scripts.Match3.Game.UI;

namespace _Project.Scripts.Match3.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        private BoardManager _boardManager;
        private InputManager _inputManager;
        private UIManager _uiManager;
        private ScoreManager _scoreManager;
        
        protected override void SetupManagers()
        {
            _boardManager = GetManager<BoardManager>();
            _inputManager = GetManager<InputManager>();
            _particleManager = GetManager<ParticleManager>();
            _scoreManager = GetManager<ScoreManager>();
            _uiManager = GetManager<UIManager>();
            
            _boardManager.Setup();
            _inputManager.Setup();
            _uiManager.Setup();
            _particleManager.Setup();
        }

        public override void Setup()
        {
            base.Setup();
            SetupEvents();
            
            StartGame();
        }
        

        private void SetupEvents()
        {
            _inputManager.ClickTileEvent += _boardManager.ClickTile;
            _inputManager.DragTileEvent += _boardManager.DragToTile;
            _inputManager.ReleaseTileEvent += _boardManager.ReleaseTile;
            
            _boardManager.ClearPiecePfxEvent += _particleManager.ClearPiecePfxAt;
            _boardManager.BreakTilePfxEvent += _particleManager.BreakTilePfxAt;
            _boardManager.BombPiecePfxEvent += _particleManager.BombPfxAt;
            _boardManager.AddScoreEvent += _scoreManager.AddScore;
            
            _boardManager.MovesLeftEvent += _uiManager.hudScreen.UpdateMovesLeftText;
            _scoreManager.UpdateScoreTextEvent += _uiManager.hudScreen.UpdateScoreText;
        }

        private void StartGame()
        {
            _boardManager.SetGameBoard();
            _uiManager.SetHUD();
        }
        
        
    }
}
