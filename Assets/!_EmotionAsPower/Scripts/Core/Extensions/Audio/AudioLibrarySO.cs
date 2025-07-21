using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrarySO", menuName = "Scriptable Objects/AudioLibrarySO")]
public class AudioLibrarySO : ScriptableObject
{
    [SerializeField] private SoundData[] soundEffects;
    [SerializeField] private SoundData[] music;

    private Dictionary<string, SoundData> soundEffectDictionary;
    private Dictionary<string, SoundData> musicDictionary;
    public void Initialize()
    {
        ConvertDataToDictionary();
    }

    //Mapping from array to dictionary to avoid duplicated id
    private void ConvertDataToDictionary()
    {
        soundEffectDictionary = new Dictionary<string, SoundData>();

        for (int i = 0; i < soundEffects.Length; i++)
        {
            SoundData soundData = soundEffects[i];
            if (soundData != null && !soundEffectDictionary.ContainsKey(soundData.soundDataID))
            {
                soundEffectDictionary.Add(soundData.soundDataID, soundData);
            }
        }

        musicDictionary = new Dictionary<string, SoundData>();

        for (int i = 0; i < music.Length; i++)
        {
            SoundData musicData = music[i];
            if (musicData != null && !soundEffectDictionary.ContainsKey(musicData.soundDataID)
                && !musicDictionary.ContainsKey(musicData.soundDataID))
            {
                musicDictionary.Add(musicData.soundDataID, musicData);
            }
        }
    }

    public SoundData GetSoundEffect(string soundDataID)
    {
        SoundData soundData = null;
        soundEffectDictionary.TryGetValue(soundDataID, out soundData);
        if (soundData == null)
        {
            Debug.LogError($"SoundDataID not found in the AudioLibrarySO: {soundDataID}");
        }
        return soundData;
    }

    public SoundData GetMusic(string soundDataID)
    {
        SoundData soundData = null;
        musicDictionary.TryGetValue(soundDataID, out soundData);
        if (soundData == null)
        {
            Debug.LogError($"SoundDataID not found in the AudioLibrarySO: {soundDataID}");
        }
        return soundData;
    }
}