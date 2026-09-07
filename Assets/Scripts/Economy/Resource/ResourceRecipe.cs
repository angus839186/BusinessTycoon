using System;
using UnityEngine;

[Serializable]
public sealed class ResourceRecipe
{
    public ResourceAmount[] inputResources;
    public ResourceAmount[] outputResources;
}