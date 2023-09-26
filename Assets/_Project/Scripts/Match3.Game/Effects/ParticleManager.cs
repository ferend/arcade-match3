using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.Effects
{
    public class ParticleManager : Manager
    {
        [SerializeField] private GameObject clearPfxPrefab;
        [SerializeField] private GameObject breakPfxPrefab;
        [SerializeField] private GameObject doubleBreakPfxPrefab;
        [SerializeField] private GameObject bombPfxPrefab;

        private void CreateParticle(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                GameObject particleFX = Instantiate(prefab, position, Quaternion.identity);
                ParticlePlayer particlePlayer = particleFX.GetComponent<ParticlePlayer>();
                particlePlayer.PlayParticle();
            }
        }

        public void ClearPiecePfxAt(int x, int y, int z = 0)
        {
            CreateParticle(clearPfxPrefab, new Vector3(x, y, z));
        }

        public void BreakTilePfxAt(int breakableVal, int x, int y, int z = 0)
        {
            GameObject prefab = (breakableVal > 1 && doubleBreakPfxPrefab != null) ? doubleBreakPfxPrefab : breakPfxPrefab;
            CreateParticle(prefab, new Vector3(x, y, z));
        }

        public void BombPfxAt(int x, int y, int z = 0)
        {
            GameObject bombPfx = Instantiate(bombPfxPrefab, new Vector3(x,y,z), Quaternion.identity);
            ParticlePlayer particlePlayer = bombPfx.GetComponent<ParticlePlayer>();
            if (particlePlayer != null)
            {
                particlePlayer.PlayParticle();
            }
            

        }
    }
}