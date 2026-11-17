using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SfxLibrary",
    menuName = "Game/Audio/SFX Library"
)]
public sealed class SfxLibrary :
    ScriptableObject
{
    [SerializeField]
    private SfxEntry[] entries =
        Array.Empty<SfxEntry>();

    public bool TryGet(
        SfxId id,
        out AudioClip clip,
        out float volume
    )
    {
        foreach (SfxEntry entry in entries)
        {
            if (
                entry == null ||
                entry.id != id ||
                entry.clip == null
            )
            {
                continue;
            }

            clip = entry.clip;
            volume = Mathf.Clamp01(
                entry.volume
            );

            return true;
        }

        clip = null;
        volume = 1f;

        return false;
    }
}