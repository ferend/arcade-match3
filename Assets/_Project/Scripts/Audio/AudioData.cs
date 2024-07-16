using UnityEngine;

namespace _Project.Scripts.Match3.Game.Sound
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "CustomAudio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [SerializeField]
        private AudioClip clip;

        [SerializeField]
        public float volume = 1.0f;

        [Range (-2, 4)]
        [SerializeField]
        public float pitch = 1.0f;

        [SerializeField]
        public bool randomPitch;

        public string AudioName;

        public bool isMusic;

        public AudioClip GetClip() => clip;
    } 
}