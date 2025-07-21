using System;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioLibrarySO audioLibrarySO;
    IObjectPool<SoundEmitter> soundEmitterPool;
    readonly List<SoundEmitter> activeSoundEmitters = new List<SoundEmitter>();
    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new Queue<SoundEmitter>();
    [SerializeField] SoundEmitter soundEmitterPrefab;
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaulCapacity = 10;
    [SerializeField] int maxPoolSize = 100;
    [SerializeField] int maxSoundInstances = 30;

    protected override void Awake()
    {
        base.Awake();
        if (audioLibrarySO == null)
        {
            Debug.LogError("Missing AudioLibraySO");
            return;
        }

        audioLibrarySO.Initialize();

        InitializeObjectPool();
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);

    public bool CanPlaySound(SoundData data)
    {
        if (!data.frequentSound) return true;
        if (FrequentSoundEmitters.Count >= maxSoundInstances && FrequentSoundEmitters.TryDequeue(out var soundEmitter))
        {
            try
            {
                soundEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("Sound Emitter is already released");
            }
            return false;
        }
        return true;
    }

    public SoundEmitter Get()
    {
        // Safety check
        if (soundEmitterPool == null)
        {
            Debug.LogError("SoundEmitter pool is not initialized!");
            return null;
        }
        return soundEmitterPool.Get();
    }

    private void InitializeObjectPool()
    {
        soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaulCapacity,
                maxPoolSize
            );
    }

    public void ReturnToPool(SoundEmitter emitter)
    {
        if (emitter != null && soundEmitterPool != null)
        {
            soundEmitterPool.Release(emitter);
        }
    }

    private SoundEmitter CreateSoundEmitter()
    {
        SoundEmitter soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }

    private void OnTakeFromPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(true);
        activeSoundEmitters.Add(soundEmitter);
    }

    private void OnReturnedToPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(soundEmitter);
    }

    protected override void OnDestroy()
    {
        // Clear the pool to prevent cleanup errors
        soundEmitterPool?.Clear();
        activeSoundEmitters.Clear();
        FrequentSoundEmitters.Clear();
        base.OnDestroy();
    }

    private void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {
        if (soundEmitter != null && soundEmitter.gameObject != null)
        {
            Destroy(soundEmitter.gameObject);
        }
    }

    public void PlaySoundEffectFromLibrary(string SoundDataID)
    {
        SoundData soundData = audioLibrarySO.GetSoundEffect(SoundDataID);
        CreateSound().WithSoundData(soundData).WithRandomPitch().Play();
    }

    public void PlayMusicFromLibrary(string SoundDataID)
    {
        SoundData soundData = audioLibrarySO.GetMusic(SoundDataID);
        CreateSound().WithSoundData(soundData).WithRandomPitch().Play();
    }

    public void StopAllSounds()
    {
        // Create a copy to avoid modifying collection during iteration
        var emittersToStop = new List<SoundEmitter>(activeSoundEmitters);

        foreach (var emitter in emittersToStop)
        {
            try
            {
                emitter.Stop();
                ReturnToPool(emitter);
            }
            catch
            {
                Debug.LogWarning("Failed to stop SoundEmitter, possibly already realised");
            }
        }

        FrequentSoundEmitters.Clear();
    }

    public void StopSound(string soundDataID)
    {
        // Create a copy to avoid modifying collection during iteration
        var emittersToStop = new List<SoundEmitter>(activeSoundEmitters);

        foreach (var emitter in emittersToStop)
        {
            if (emitter != null && emitter.Data.soundDataID == soundDataID)
            {
                try
                {
                    emitter.Stop();
                    ReturnToPool(emitter);
                }
                catch
                {
                    Debug.LogWarning($"Failed to stop SoundEmitter with SoundDataID: {soundDataID}, possibly already realised");
                }
            }
        }

        //Remove from FrequentSoundEmitters if present
        var tempQueue = new Queue<SoundEmitter>();
        while (FrequentSoundEmitters.Count > 0)
        {
            var emitter = FrequentSoundEmitters.Dequeue();
            if (emitter != null && emitter.Data.soundDataID != soundDataID)
            {
                tempQueue.Enqueue(emitter);
            }
        }
        while (tempQueue.Count > 0)
        {
            FrequentSoundEmitters.Enqueue(tempQueue.Dequeue());
        }

    }
}