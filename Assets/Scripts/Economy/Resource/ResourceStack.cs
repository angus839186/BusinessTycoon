using System;
using UnityEngine;

[Serializable]
public sealed class ResourceStack
{
    public ResourceDefinition resource;
    public int amount;

    public ResourceStack(ResourceDefinition resource, int amount)
    {
        this.resource = resource;
        this.amount = amount;
    }
}