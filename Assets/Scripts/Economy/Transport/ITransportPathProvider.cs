using System.Collections.Generic;
using UnityEngine;

public interface ITransportPathProvider
{
    IReadOnlyList<Vector3> GetPath(
        BuildingInstance source,
        BuildingInstance target
    );
}