using System.Collections.Generic;

public sealed class BuildingInventory
{
    private readonly List<ResourceStack> resources = new List<ResourceStack>();

    public IReadOnlyList<ResourceStack> Resources => resources;

    public int GetAmount(ResourceDefinition resource)
    {
        ResourceStack stack = FindStack(resource);
        return stack != null ? stack.amount : 0;
    }

    public void Add(ResourceDefinition resource, int amount)
    {
        if (resource == null || amount <= 0)
            return;

        ResourceStack stack = FindStack(resource);

        if (stack == null)
        {
            resources.Add(new ResourceStack(resource, amount));
            return;
        }

        stack.amount += amount;
    }

    public bool TryConsume(ResourceDefinition resource, int amount)
    {
        if (resource == null || amount <= 0)
            return false;

        ResourceStack stack = FindStack(resource);

        if (stack == null || stack.amount < amount)
            return false;

        stack.amount -= amount;
        return true;
    }

    private ResourceStack FindStack(ResourceDefinition resource)
    {
        foreach (ResourceStack stack in resources)
        {
            if (stack.resource == resource)
                return stack;
        }

        return null;
    }
    public bool HasEnough(ResourceAmount[] costs)
    {
        if (costs == null)
            return true;

        foreach (ResourceAmount cost in costs)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            if (GetAmount(cost.resource) < cost.amount)
                return false;
        }

        return true;
    }

    public bool TryConsumeAll(ResourceAmount[] costs)
    {
        if (!HasEnough(costs))
            return false;

        if (costs == null)
            return true;

        foreach (ResourceAmount cost in costs)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            TryConsume(cost.resource, cost.amount);
        }

        return true;
    }
    public void Clear()
    {
        resources.Clear();
    }
}