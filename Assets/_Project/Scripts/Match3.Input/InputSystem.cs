using _Project.Scripts.Match3.Game.TileActor;
using UnityEngine;

namespace _Project.Scripts.Match3.Input
{
    /// <summary>
    /// Needs improvement.
    /// </summary>
    public class InputSystem : global::Game.System
    {
        private Tile _currentTile;
        [SerializeField] private Camera gameCamera;
        private int _layerMask;

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
                    _currentTile.OnEnter();
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_currentTile != null) 
                { 
                    _currentTile.OnDown(); 
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _currentTile != null)
            {
                _currentTile.OnUp();
                _currentTile = null;
            }
        }
    }
}