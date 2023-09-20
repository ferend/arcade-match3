using _Project.Scripts.Match3.Game.BoardActor;
using _Project.Scripts.Match3.Game.Effects;
using UnityEngine;

namespace _Project.Scripts.Match3.Game
{
    public class GameplaySystem : global::Game.System
    {
        private ParticleManager particleManager;
        [SerializeField] private Board board;
        
        protected override void SetupManagers()
        {
            base.SetupManagers();
            particleManager = GetManager<ParticleManager>();
        }

        public override void Setup()
        {
            base.Setup();
            SetupEvents();
        }

        private void SetupEvents()
        {
            board.ClearPiecePFXEvent += particleManager.ClearPiecePFXAt;
            board.BreakTilePFXEvent += particleManager.BreakTilePFXAt;
        }
        
        
    }
}
