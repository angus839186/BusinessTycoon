using UnityEngine;

public sealed class BuildingPlacementPreview : MonoBehaviour
{
    [SerializeField] private float previewSize = 0.18f;
    [SerializeField] private Color validColor = new Color(0f, 1f, 0.2f, 0.65f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.1f, 0.1f, 0.65f);
    [SerializeField] private int sortingOrder = 500;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite();
        spriteRenderer.sortingOrder = sortingOrder;
        transform.localScale = Vector3.one * previewSize;
        Hide();
    }

    public void Show(Vector3 worldPosition, bool canBuild)
    {
        worldPosition.z = -1f;
        transform.position = worldPosition;
        spriteRenderer.color = canBuild ? validColor : invalidColor;
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private Sprite CreateSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f
        );
    }
}