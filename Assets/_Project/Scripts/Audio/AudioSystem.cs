using System.Collections.Generic;
using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Audio
{
    public class AudioSystem : global::Game.System
    {
        [SerializeField] private List<AudioSource> sources;
        [SerializeField] private List<AudioSource> oneShotSources;

        [SerializeField] private AudioClip popSoundClip;
        [SerializeField] private AudioClip bombSoundClip;

        [SerializeField] private AudioDataHolder collection;
        public Dictionary<string, AudioData> AudioDict = new Dictionary<string, AudioData>() { };
        
        internal bool mutedMusic = false;
        internal bool mutedSFX = false;

        public void Awake()
        {
            ServiceLocator.RegisterService(this);

            if (collection == null) return;

            foreach (AudioData audio in collection.GetCollection())
            {
                AudioDict.Add(audio.AudioName, audio);
            }
        }
        private bool IsMusic(string clipName)
        {
            foreach (AudioData audio in collection.GetCollection())
            {
                if (audio.GetClip().name == clipName)
                {
                    return audio.isMusic;
                }
            }
            return false;
        }
        
        private void MuteMusicSources(bool mute)
        {
            foreach (AudioSource source in sources)
            {
                if (IsMusic(source.clip.name))
                    source.mute = mute;
            }
        }
        
        private void MuteSfxSources(bool mute)
        {
            foreach (AudioSource source in sources)
            {
                if (!IsMusic(source.clip.name))
                    source.mute = mute;
            }
        }
        public void Play(string audioName, bool loop)
        {
            AudioSource source = PrepareSource(audioName);

            source.loop = loop;

            if (IsMusic(source.clip.name)) source.mute = mutedMusic;
            else source.mute = mutedSFX;

            source.Play();
        }
        
        public void PlayBombFX()
        {
            PlayOneShotClip(bombSoundClip,Vector3.zero,1f,1f);
        }

        public void PlayPopFX()
        {
            PlayOneShotClip(popSoundClip,Vector3.zero,1f,1f);
        }
        
        private void PlayOneShotClip(AudioClip clip, Vector3 position, float volume, float minDistance)
        {
            if (mutedSFX)
                return;

            AudioSource source = oneShotSources[0];
            source.spatialBlend = 1.0f;
            source.clip = clip;
            source.volume = volume;
            source.minDistance = minDistance;
            source.Play();
        }
        private AudioSource PrepareSource(string audioName)
        {
            AudioData audio = AudioDict[audioName];
            AudioSource source = GetSource();
            source.clip = audio.GetClip();
            source.volume = audio.volume;
            if (!audio.randomPitch)
                source.pitch = audio.pitch;
            else
                source.pitch = Random.Range(0.9f, 1.3f);

            return source;
        }
        private AudioSource GetSource()
        {
            foreach (AudioSource source in sources)
            {
                if (source != null)
                {
                    if (!source.isPlaying)
                    {
                        source.loop = false;
                        return source;
                    }
                }
            }
            return CreateSource();
        }

        private AudioSource CreateSource()
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            sources.Add(newSource);
            return newSource;
        }

        /// <param name="audioName">Name of the AudioData obj</param>
        /// <param name="loop">Boolean for the loop</param>
        public void Play(string audioName, bool loop, bool unique)
        {
            if (unique && !CheckDuplicateClip(AudioDict[audioName].GetClip().name))
            {
                AudioSource source = PrepareSource(audioName);

                source.loop = loop;
                if (IsMusic(source.clip.name)) source.mute = mutedMusic;
                else source.mute = mutedSFX;

                source.Play();
            }
        }
        
        /// <param name="clipName"></param>
        /// <returns>true if duplicate exists, false otherwise</returns>
        private bool CheckDuplicateClip(string clipName)
        {
            foreach (AudioSource source in sources)
            {
                if (source.clip.name == clipName && source.isPlaying)
                    return true;
            }
            return false;
        }
        public void Stop(string clipName)
        {
            AudioClip clip = AudioDict[clipName].GetClip();
            foreach (AudioSource source in sources)
            {
                if (source != null)
                {
                    if (source.clip == clip && source.isPlaying)
                    {
                        source.Stop();
                    }
                }

            }
        }
        public void StopAll()
        {
            foreach (AudioSource source in sources)
            {
                if (source != null && source.isPlaying) source.Stop();
            }
        }

    }
}