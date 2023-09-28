using System;
using _Project.Scripts.Match3.Game.TileActor;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Input
{
    public class InputManager : Manager
    {
        private Tile _currentTile;
        [SerializeField] private Camera gameCamera;
        private int _layerMask;

        public event Action<Tile> DragTileEvent;
        public event Action<Tile> ClickTileEvent;
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
                _currentTile = hit.collider.GetComponent<Tile>();
                
                if (_currentTile != null)
                {
                    DragTileEvent?.Invoke(_currentTile);
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_currentTile != null) 
                { 
                    ClickTileEvent?.Invoke(_currentTile); 
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _currentTile != null)
            {
                ReleaseTileEvent?.Invoke();
                _currentTile = null;
            }
        }
    }
}