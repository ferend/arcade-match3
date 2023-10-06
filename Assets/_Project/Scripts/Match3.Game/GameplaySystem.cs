using _Project.Scripts.Match3.Game.BoardActor;
using _Project.Scripts.Match3.Game.Effects;
using _Project.Scripts.Match3.Game.Input;
using _Project.Scripts.Match3.Game.Player;
using _Project.Scripts.Match3.Game.Sound;
using _Project.Scripts.Match3.Game.UI;
using UnityEngine;

namespace _Project.Scripts.Match3.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        private BoardManager _boardManager;
        private InputManager _inputManager;
        private UIManager _uiManager;
        private ScoreManager _scoreManager;
        private SoundManager _soundManager;
        
        protected override void SetupManagers()
        {
            _boardManager = GetManager<BoardManager>();
            _inputManager = GetManager<InputManager>();
            _particleManager = GetManager<ParticleManager>();
            _scoreManager = GetManager<ScoreManager>();
            _uiManager = GetManager<UIManager>();
            _soundManager = GetManager<SoundManager>();
            
            _boardManager.Setup();
            _inputManager.Setup();
            _uiManager.Setup();
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
            _boardManager.AddScoreEvent += _scoreManager.AddScore;
            _boardManager.MovesLeftEvent += _uiManager.hudScreen.UpdateMovesLeftText;
            _boardManager.GameStatusEvent += ControlGameStatus;
            _boardManager.PlayPopSoundEvent += _soundManager.PlayPopFX;
            _boardManager.PlayBombSoundEvent += _soundManager.PlayBombFX;
            
            _scoreManager.UpdateScoreTextEvent += _uiManager.hudScreen.UpdateScoreText;
        }

        private void StartGame()
        {
            _boardManager.SetGameBoard();
            _uiManager.SetHUD();
            _soundManager.Play("Main",true);
        }

        private void ControlGameStatus()
        {

            if (_boardManager.board.movesLeft > 0 && _scoreManager.GetCurrentScore() >= _boardManager.board.targetScoreToWin)
            {
                _boardManager.CloseGameBoard();
                _uiManager.SetWinScreen();
                _boardManager.canGetInput = false;
                _boardManager.canDropTiles = false;
                _soundManager.StopAll();
                _soundManager.Play("Win",false);
            }
            if (_boardManager.board.movesLeft == 0)
            {                
                _boardManager.CloseGameBoard();
                _boardManager.canGetInput = false;
                _boardManager.canDropTiles = false;
                _soundManager.StopAll();
            }
            

        }
        
        
    }
}
