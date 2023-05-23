using UnityEngine;

namespace _Project.Scripts.Match3.Input
{
    public interface IInputHandler
    {
        void OnDown(Vector3 pos);
        void OnDrag(Vector3 pos);
        void OnUp();
    }
}