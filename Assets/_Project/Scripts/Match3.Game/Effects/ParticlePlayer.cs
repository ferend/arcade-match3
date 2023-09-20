using UnityEngine;

namespace _Project.Scripts.Match3.Game.Effects
{
    public class ParticlePlayer : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] allParticles;
        [SerializeField] private float lifetime = 1f;

        private void Start()
        {
            allParticles = GetComponentsInChildren<ParticleSystem>();
            Destroy(gameObject , lifetime);
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
