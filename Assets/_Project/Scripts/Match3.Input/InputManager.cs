using _Project.Scripts.Match3.Actor;
using UnityEngine;

namespace _Project.Scripts.Match3.Input
{
    /// <summary>
    /// Needs improvement.
    /// </summary>
    public class InputManager : Manager
    {
        private Tile currentTile;
        [SerializeField] private Camera gameCamera;
        private int layerMask;

        public override void Setup()
        {
            layerMask = LayerMask.GetMask("Board");
        }

        public override void Tick(float deltaTime)
        {
                   
            RaycastHit2D hit = Physics2D.Raycast(gameCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                currentTile = hit.collider.GetComponent<Tile>();
                
                if (currentTile != null)
                {
                    currentTile.OnEnter();
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (currentTile != null) 
                { 
                    currentTile.OnDown(); 
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && currentTile != null)
            {
                currentTile.OnUp();
                currentTile = null;
            }
        }
    }
}