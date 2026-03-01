using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SignalMenu.SignalSafety
{
    public static class AudioManager
    {
        private static AudioSource _source;
        private static string soundsFolder;
        private static readonly Queue<QueuedSound> _queue = new Queue<QueuedSound>();
        private static bool _isPlaying = false;
        private static int _currentPriority = 0;
        private static int _generation = 0;

        public enum AudioCategory
        {
            Protection,
            Warning,
            Ban,
            Toggle,
            MenuDetection,
            PatchOverride
        }

        private struct QueuedSound
        {
            public string path;
            public AudioType type;
            public int priority;
            public float volume;
        }

        private static int GetPriority(AudioCategory cat)
        {
            switch (cat)
            {
                case AudioCategory.Ban: return 3;
                case AudioCategory.Warning: return 2;
                case AudioCategory.MenuDetection: return 2;
                case AudioCategory.Protection: return 1;
                case AudioCategory.PatchOverride: return 1;
                case AudioCategory.Toggle: return 0;
                default: return 0;
            }
        }

        private static float GetCategoryVolume(AudioCategory cat)
        {
            float master = SafetyConfig.AudioVolume;
            switch (cat)
            {
                case AudioCategory.Protection: return master * SafetyConfig.ProtectionVolume;
                case AudioCategory.Warning: return master * SafetyConfig.WarningVolume;
                case AudioCategory.Ban: return master * SafetyConfig.BanVolume;
                case AudioCategory.MenuDetection: return master * SafetyConfig.WarningVolume;
                case AudioCategory.PatchOverride: return master * SafetyConfig.ProtectionVolume;
                case AudioCategory.Toggle: return master * 0.5f;
                default: return master;
            }
        }

        private static AudioSource GetSource()
        {
            if (_source == null || _source.gameObject == null)
            {
                var obj = new GameObject("_at" + UnityEngine.Random.Range(1000, 9999));
                GameObject.DontDestroyOnLoad(obj);
                _source = obj.AddComponent<AudioSource>();
                _source.volume = 1f;
                _source.playOnAwake = false;
            }
            return _source;
        }

        public static void Play(string name, AudioCategory category = AudioCategory.Protection)
        {
            if (!SafetyConfig.AudioEnabled) return;

            if (category == AudioCategory.Toggle) return;

            switch (category)
            {
                case AudioCategory.Protection:
                    if (!SafetyConfig.PlayProtectionAudio) return;
                    break;
                case AudioCategory.Warning:
                    if (!SafetyConfig.PlayWarningAudio) return;
                    break;
                case AudioCategory.Ban:
                    if (!SafetyConfig.PlayBanAudio) return;
                    break;
                case AudioCategory.MenuDetection:
                    if (!SafetyConfig.PlayMenuDetectionAudio) return;
                    break;
                case AudioCategory.PatchOverride:
                    if (!SafetyConfig.PlayPatchOverrideAudio) return;
                    break;
            }

            float volume = GetCategoryVolume(category);

            try
            {
                if (soundsFolder == null)
                    soundsFolder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "nexussounds");

                string[] extensions = { ".wav", ".mp3" };
                foreach (var ext in extensions)
                {
                    string path = Path.Combine(soundsFolder, name + ext);
                    if (File.Exists(path))
                    {
                        int prio = GetPriority(category);

                        if (_isPlaying && prio > _currentPriority)
                        {
                            GetSource().Stop();
                            _isPlaying = false;
                            _generation++;
                            _queue.Clear();
                        }

                        _queue.Enqueue(new QueuedSound { path = path, type = ext == ".mp3" ? AudioType.MPEG : AudioType.WAV, priority = prio, volume = volume });

                        if (!_isPlaying && Plugin.Instance != null)
                            Plugin.Instance.StartCoroutine(ProcessQueue());

                        return;
                    }
                }
            }
            catch { }
        }

        private static IEnumerator ProcessQueue()
        {
            _isPlaying = true;
            int myGeneration = _generation;

            while (_queue.Count > 0)
            {
                if (myGeneration != _generation)
                {
                    yield break;
                }

                var sound = _queue.Dequeue();
                _currentPriority = sound.priority;

                AudioClip clip = null;

                var request = UnityWebRequestMultimedia.GetAudioClip("file://" + sound.path, sound.type);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    clip = DownloadHandlerAudioClip.GetContent(request);

                request.Dispose();

                if (clip != null)
                {
                    var src = GetSource();
                    src.clip = clip;
                    src.volume = sound.volume;
                    src.Play();

                    while (src.isPlaying)
                    {
                        if (myGeneration != _generation) yield break;
                        yield return new WaitForSeconds(0.1f);
                    }

                    yield return new WaitForSeconds(0.15f);
                }
            }

            _isPlaying = false;
            _currentPriority = 0;
        }
    }
}
