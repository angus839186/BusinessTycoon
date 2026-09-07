using UnityEngine;
using System.Collections.Generic;
using System;

public sealed partial class TransportSystem : MonoBehaviour
{
    [SerializeField] private GameRules gameRules;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private GameTimeInterval transferInterval;
    [SerializeField] private int transferAmount = 1;
    [SerializeField] private bool logTransfer = true;
    [SerializeField] private TransportRouteVisualizer routeVisualizer;

    [SerializeField] private bool useDefinedRoutes = false;
    [SerializeField] private TransportRouteDefinition[] routes;

    public IReadOnlyList<TransportRouteDefinition> Routes =>
    routes ?? Array.Empty<TransportRouteDefinition>();

    [SerializeField] private GameClock gameClock;

    private readonly List<TransportRecord> transportRecords = new List<TransportRecord>();

    [SerializeField] private int maxTransportRecords = 50;

    public IReadOnlyList<TransportRecord> TransportRecords =>
        transportRecords;
    public int RouteVersion { get; private set; }

    [SerializeField] private MonoBehaviour pathProviderBehaviour;

    private ITransportPathProvider pathProvider;

    private double lastTransferGameMinutes;
    public double LastTransferGameMinutes => lastTransferGameMinutes;
    public int NextRouteIndex => nextRouteIndex;

    private int nextRouteIndex = 1;

    public event Action RoutesChanged;
    public event Action TransportChanged;

    private void Awake()
    {
        if (gameRules == null)
            gameRules = FindFirstObjectByType<GameRules>();
        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
        if (routeVisualizer == null)
            routeVisualizer = FindFirstObjectByType<TransportRouteVisualizer>();
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        pathProvider = pathProviderBehaviour as ITransportPathProvider;

        if (pathProvider == null)
            pathProvider = FindFirstObjectByType<TemporaryTransportPathProvider>();

        if (gameClock != null)
            lastTransferGameMinutes = gameClock.TotalGameMinutes;
    }

    private void Update()
    {
        if (productionSystem == null)
            return;

        if (gameClock == null)
            return;

        double intervalMinutes = transferInterval.ToMinutes();

        if (gameRules != null)
            intervalMinutes *= gameRules.TransportIntervalMultiplier;

        if (intervalMinutes <= 0)
            return;

        double elapsedMinutes = gameClock.TotalGameMinutes - lastTransferGameMinutes;

        if (elapsedMinutes < intervalMinutes)
            return;

        lastTransferGameMinutes = gameClock.TotalGameMinutes;

        if (useDefinedRoutes)
            TransferDefinedRouteOnce();
        else
            TransferOnce();
    }
    private BuildingInstance FindBuildingById(string buildingId)
    {
        if (string.IsNullOrWhiteSpace(buildingId) || productionSystem == null)
            return null;

        string trimmedId = buildingId.Trim();

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null)
                continue;

            if (building.InstanceId == trimmedId)
                return building;

            if (building.gameObject.name == trimmedId)
                return building;
        }

        return null;
    }
}