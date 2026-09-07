using UnityEngine;

public static class GeoJsonMaterialFactory
{
    public static Material Create(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        Material material = new Material(shader);
        material.color = color;
        material.renderQueue = 3000;
        return material;
    }
}