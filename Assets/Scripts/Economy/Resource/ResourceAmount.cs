using System;
using UnityEngine;

[Serializable]
public sealed class ResourceAmount
{
    public ResourceDefinition resource;
    public int amount = 1;
}