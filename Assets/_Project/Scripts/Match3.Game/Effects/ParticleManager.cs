using System;
using UnityEngine;

namespace _Project.Scripts.Match3.Game.Effects
{
    public class ParticleManager : Manager
    {
        [SerializeField] private GameObject clearPFXPrefab;
        [SerializeField] private GameObject breakPFXPrefab;
        [SerializeField] private GameObject doubleBreakPFXPrefab;
        

        public void ClearPiecePFXAt(int x, int y, int z = 0)
        {
            if (clearPFXPrefab != null)
            {
                GameObject clearPFX = Instantiate(clearPFXPrefab, new Vector3(x, y, z), Quaternion.identity);

                ParticlePlayer particlePlayer = clearPFX.GetComponent<ParticlePlayer>();
                
                particlePlayer.PlayParticle();
            }
        }

        public void BreakTilePFXAt(int breakableVal, int x, int y, int z = 0)
        {
            GameObject breakFX = null;
            ParticlePlayer particlePlayer = null;

            if (breakableVal > 1 && doubleBreakPFXPrefab != null)
            {
                breakFX = Instantiate(doubleBreakPFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            }
            else
            {
                if (breakPFXPrefab != null)
                {
                    breakFX = Instantiate(breakPFXPrefab, new Vector3(x, y, z), Quaternion.identity);
                }
            }

            if (breakFX != null)
            {
                particlePlayer = breakFX.GetComponent<ParticlePlayer>();
                particlePlayer.PlayParticle();
            }
        }
    }
}