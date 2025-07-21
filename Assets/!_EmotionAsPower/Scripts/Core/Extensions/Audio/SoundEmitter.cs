
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    public SoundData Data { get; private set; }
    AudioSource audioSource;
    Coroutine playingCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }

        audioSource.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);

            audioSource.Stop();
            AudioManager.Instance.ReturnToPool(this);
        }
    }

    public void Initialize(SoundData soundData)
    {
        Data = soundData;
        audioSource.clip = soundData.clip;
        audioSource.outputAudioMixerGroup = soundData.audioMixerGroup;
        audioSource.loop = soundData.loop;
        audioSource.playOnAwake = soundData.playOnAwake;
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        AudioManager.Instance.ReturnToPool(this);
    }

    public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        audioSource.pitch += Random.Range(min, max);
    }
}