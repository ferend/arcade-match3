using UnityEngine;

namespace _Project.Scripts.Match3
{
    public abstract class Manager : MonoBehaviour
    {
        public virtual void Setup()
        { }

        public virtual void Dispose()
        { }

        public virtual void Tick(float deltaTime)
        { }
    }
}

