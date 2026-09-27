using System.Collections.Generic;
using UnityEngine;

public sealed class ReusableUIList<TItem>
    where TItem : Component
{
    private readonly TItem prefab;
    private readonly Transform root;
    private readonly List<TItem> items = new List<TItem>();

    public ReusableUIList(TItem prefab, Transform root)
    {
        this.prefab = prefab;
        this.root = root;
    }

    public TItem GetOrCreate(int index)
    {
        if (index < 0 || prefab == null || root == null)
            return null;

        while (items.Count <= index)
        {
            TItem item = Object.Instantiate(prefab, root);
            item.gameObject.SetActive(false);
            items.Add(item);
        }

        items[index].gameObject.SetActive(true);
        return items[index];
    }

    public void HideFrom(int firstUnusedIndex)
    {
        firstUnusedIndex = Mathf.Max(0, firstUnusedIndex);

        for (int i = firstUnusedIndex; i < items.Count; i++)
            items[i].gameObject.SetActive(false);
    }
}