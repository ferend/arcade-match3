using UnityEngine;

namespace _Project.Scripts.Game.Player
{
    public interface IInputHandler
    {
        void OnDown(Vector3 pos);
        void OnDrag(Vector3 pos);
        void OnUp();
    }
}