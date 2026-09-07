using System.Collections.Generic;
using UnityEngine;

public sealed class TemporaryTransportPathProvider : MonoBehaviour, ITransportPathProvider
{
    public IReadOnlyList<Vector3> GetPath(
        BuildingInstance source,
        BuildingInstance target
    )
    {
        if (source == null || target == null)
            return null;

        Vector3 sourcePosition = source.transform.position;
        Vector3 targetPosition = target.transform.position;

        return new List<Vector3>
{
    sourcePosition,
    targetPosition
};
    }
}