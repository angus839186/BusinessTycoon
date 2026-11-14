using UnityEngine;

[CreateAssetMenu(
    fileName = "LandValueConfig",
    menuName = "Business Tycoon/Balance/Land Value Config"
)]
public sealed class LandValueConfig : ScriptableObject
{
    [Header("Land Score Formula")]

    [Min(0.01f)]
    public float maxScore = 100f;

    [Min(0.01f)]
    public float densityExponent = 1.5f;

    [Min(0.01f)]
    public float midpointDensity = 5000f;
}