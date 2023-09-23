using System;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Effects
{
    public class ParticleManager : Manager
    {
        [SerializeField] private GameObject clearPFXPrefab;
        [SerializeField] private GameObject breakPFXPrefab;
        [SerializeField] private GameObject doubleBreakPFXPrefab;

        private void CreateParticle(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                GameObject particleFX = Instantiate(prefab, position, Quaternion.identity);
                ParticlePlayer particlePlayer = particleFX.GetComponent<ParticlePlayer>();
                particlePlayer.PlayParticle();
            }
        }

        public void ClearPiecePFXAt(int x, int y, int z = 0)
        {
            CreateParticle(clearPFXPrefab, new Vector3(x, y, z));
        }

        public void BreakTilePFXAt(int breakableVal, int x, int y, int z = 0)
        {
            GameObject prefab = (breakableVal > 1 && doubleBreakPFXPrefab != null) ? doubleBreakPFXPrefab : breakPFXPrefab;
            CreateParticle(prefab, new Vector3(x, y, z));
        }
    }
}