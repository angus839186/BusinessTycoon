using System;
using UnityEngine;

[Serializable]
public sealed class SfxEntry
{
    public SfxId id;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}