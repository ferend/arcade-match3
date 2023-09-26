using _Project.Scripts.Match3.Game.BoardActor;
using _Project.Scripts.Match3.Game.Effects;
using UnityEngine;

namespace _Project.Scripts.Match3.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager _particleManager;
        [SerializeField] private Board board;
        
        protected override void SetupManagers()
        {
            base.SetupManagers();
            _particleManager = GetManager<ParticleManager>();
            _particleManager.Setup();
        }

        public override void Setup()
        {
            base.Setup();
            SetupEvents();
        }

        private void SetupEvents()
        {
            board.ClearPiecePfxEvent += _particleManager.ClearPiecePfxAt;
            board.BreakTilePfxEvent += _particleManager.BreakTilePfxAt;
            board.BombPiecePfxEvent += _particleManager.BombPfxAt;
        }
        
        
    }
}
