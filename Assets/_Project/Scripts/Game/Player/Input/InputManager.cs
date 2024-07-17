using System;
using _Project.Scripts.Core;
using _Project.Scripts.Game.TileActor;
using UnityEngine;

namespace  _Project.Scripts.Game.Player
{
    public class InputManager : Manager
    {
        private TileComponent _currentTileComponent;
        [SerializeField] private Camera gameCamera;
        private int _layerMask;

        public event Action<TileComponent> DragTileEvent;
        public event Action<TileComponent> ClickTileEvent;
        public event Action ReleaseTileEvent;
        
        public override void Setup()
        {
            _layerMask = LayerMask.GetMask("Board");
        }

        public override void Tick(float deltaTime)
        {
                   
            RaycastHit2D hit = Physics2D.Raycast(gameCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition), Vector2.zero, Mathf.Infinity, _layerMask);

            if (hit.collider != null)
            {
                _currentTileComponent = hit.collider.GetComponent<TileComponent>();
                
                if (_currentTileComponent != null)
                {
                    DragTileEvent?.Invoke(_currentTileComponent);
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_currentTileComponent != null) 
                { 
                    ClickTileEvent?.Invoke(_currentTileComponent); 
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _currentTileComponent != null)
            {
                ReleaseTileEvent?.Invoke();
                _currentTileComponent = null;
            }
        }
    }
}