using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.Effects
{
    public class ParticlePlayer : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] allParticles;
        private readonly float _lifetime = 1f;

        private void Start()
        {
            allParticles = GetComponentsInChildren<ParticleSystem>();
            Destroy(gameObject , _lifetime);
        }

        public void PlayParticle()
        {
            foreach (ParticleSystem ps in allParticles)
            {
                ps.Stop();
                ps.Play();
            }
        }
    
    }
}
