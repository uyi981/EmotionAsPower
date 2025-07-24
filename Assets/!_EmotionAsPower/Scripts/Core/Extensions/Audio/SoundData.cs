using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundData
{
    public string soundDataID;
    public AudioClip clip;
    public AudioMixerGroup audioMixerGroup;
    public bool loop;
    public bool playOnAwake;
    public bool frequentSound;
}