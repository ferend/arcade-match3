﻿using UnityEngine;

namespace _Project.Scripts.Audio
{
    [CreateAssetMenu(fileName = "AudioDataCollection", menuName = "CustomAudio/AudioDataCollection")]
    public class AudioDataHolder : ScriptableObject
    {
        [SerializeField]
        private AudioData[] audioDatas;

        public AudioData[] GetCollection()
        {
            return this.audioDatas;
        }
    }
}