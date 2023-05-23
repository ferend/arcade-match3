using UnityEngine;

namespace _Project.Scripts.Match3.Input
{
    public class InputManager : Manager
    {
        [SerializeField] private Swipe swipe;
        public override void Setup()
        {
            base.Setup();
            Debug.Log("setup input manager");
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if (UnityEngine.Input.GetMouseButtonDown(0))
                swipe.OnDown(UnityEngine.Input.mousePosition);
            else if (UnityEngine.Input.GetMouseButton(0))
                swipe.OnDrag(UnityEngine.Input.mousePosition);
            else if (UnityEngine.Input.GetMouseButtonUp(0))
                swipe.OnUp();
        }
    }
}