using UnityEngine;

public sealed class GeoJsonMapProjection : MonoBehaviour
{
    [Header("Taiwan Bounds")]
    [SerializeField] private double minLongitude = 119.0;
    [SerializeField] private double maxLongitude = 122.2;
    [SerializeField] private double minLatitude = 21.8;
    [SerializeField] private double maxLatitude = 25.4;

    [Header("World Size")]
    [SerializeField] private float worldWidth = 20f;

    private float WorldHeight
    {
        get
        {
            double lonRange = maxLongitude - minLongitude;
            double latRange = maxLatitude - minLatitude;

            if (lonRange <= 0)
                return worldWidth;

            return worldWidth * (float)(latRange / lonRange);
        }
    }

    public bool TryLonLatToWorldPosition(
        double longitude,
        double latitude,
        out Vector3 worldPosition
    )
    {
        worldPosition = Vector3.zero;

        if (
            longitude < minLongitude ||
            longitude > maxLongitude ||
            latitude < minLatitude ||
            latitude > maxLatitude
        )
            return false;

        float normalizedX = (float)((longitude - minLongitude) / (maxLongitude - minLongitude));
        float normalizedY = (float)((latitude - minLatitude) / (maxLatitude - minLatitude));

        float x = (normalizedX - 0.5f) * worldWidth;
        float y = (normalizedY - 0.5f) * WorldHeight;

        worldPosition = transform.position + new Vector3(x, y, 0f);
        return true;
    }
    public bool TryWorldPositionToLonLat(
    Vector3 worldPosition,
    out double longitude,
    out double latitude
)
    {
        longitude = 0;
        latitude = 0;

        Vector3 local = worldPosition - transform.position;

        float worldHeight = WorldHeight;

        if (worldWidth <= 0f || worldHeight <= 0f)
            return false;

        double normalizedX = local.x / worldWidth + 0.5;
        double normalizedY = local.y / worldHeight + 0.5;

        if (
            normalizedX < 0 ||
            normalizedX > 1 ||
            normalizedY < 0 ||
            normalizedY > 1
        )
            return false;

        longitude = minLongitude + normalizedX * (maxLongitude - minLongitude);
        latitude = minLatitude + normalizedY * (maxLatitude - minLatitude);

        return true;
    }
}