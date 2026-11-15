using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateType = CoordinateSystem.CoordinateType;
public class HeatmapDisplay : MonoBehaviour
{
    /// <summary>
    /// How to add a point to the heatmap
    /// </summary>
    public enum HeatmapAddMode
    {
        /// <summary>
        /// Replace the point on the heatmap with the given value
        /// </summary>
        Absolute = 0,
        /// <summary>
        /// Add the given value to the current value of the point
        /// </summary>
        Add = 1
    }
    //Modified from sebastian league display class : https://www.youtube.com/watch?v=WP-Bm65Q-1Y&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=3
    [SerializeField] private Renderer _heatmapRenderer;
    //How far each point on the heatmap should represent in world space;
    [SerializeField] private float _stepValue = 0.005f;
    [SerializeField] private int[,] _vals;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Material _material;

    private CoordinateType _coordinateType;
    private void Start()
    {
        master._onCoordinateTypeUpdated += OnCoordinateTypeUpdated;
    }
    /// <summary>
    /// Initializes the heatmap with the size calculated from the given points.
    /// </summary>
    /// <param name="worldspaceWidth">The width of the object the heatmap is representing in world space.</param>
    /// <param name="worldspaceHeight">The height of the object the heatmap is representing in world space.</param>
    public void InitializeHeatmap(float worldspaceWidth, float worldspaceHeight)
    {
        int width = (int)(worldspaceWidth / _stepValue);
        int height = (int)(worldspaceHeight / _stepValue);
        _vals = new int[width, height];
        transform.localScale = new Vector3(0.05f, 0, 0.05f * (worldspaceHeight / worldspaceWidth));
        transform.eulerAngles = new Vector3(-80, 0, 0);
        _heatmapRenderer.sharedMaterial = _material;
        DrawHeatmap();
    }
    /// <summary>
    /// Replace a singular point on the heatmap
    /// </summary>
    /// <param name="x">X coord to replace</param>
    /// <param name="z">Z coord to replace</param>
    /// <param name="strength">Strength of the point, between 0 and 1</param>
    /// <param name="mode">How the new point should affect the headmap</param>
    public void AddDotToHeatmap(float x, float z, float strength, HeatmapAddMode mode)
    {
        int coordX = _vals.GetLength(0) - (int)(x * _vals.GetLength(0));
        int coordZ = _vals.GetLength(1) - (int)(z * _vals.GetLength(1));
        if (coordX >= _vals.GetLength(0)) return;
        if (coordZ >= _vals.GetLength(1)) return;
        if(mode == HeatmapAddMode.Add) _vals[coordX, coordZ] += (int)(strength * 100);
        else _vals[coordX, coordZ] = (int)(strength * 100);
        if (_vals[coordX, coordZ] > 100) _vals[coordX, coordZ] = 100;
    }
    /// <summary>
    /// Adds a point to the heatmap, where the heatmap is expected to be normalized between 0 and 1.
    /// </summary>
    /// <param name="x"> normalized X value of the point, between 0 and 1</param>
    /// <param name="z"> normalized Z value of the point, between 0 and 1</param>
    /// <param name="span">How many pixels each quarter of the heatmap diamond should span.</param>
    public void AddPointToValsNormalized(float x, float z, int span)
    {

        int coordX = _vals.GetLength(0) - (int)(x * _vals.GetLength(0));
        int coordZ = _vals.GetLength(1) - (int)(z * _vals.GetLength(1));

        float maxDistance = Vector2.Distance(new Vector2(span, span), Vector2.zero);
        for (int x1 = -span; x1 < span; x1++)
        {
            int adjustedX = coordX + x1;
            if (adjustedX >= _vals.GetLength(0)) continue;
            if (adjustedX < 0) continue;
            for (int y1 = -span; y1 < span; y1++)
            {
                int adjustedY = coordZ + y1;
                if (adjustedY >= _vals.GetLength(1)) continue;
                if (adjustedY < 0) continue;

                float addAmount = Vector2.Distance(new Vector2(x1, y1), Vector2.zero);
                addAmount = maxDistance - addAmount;
                _vals[adjustedX, adjustedY] += (int)addAmount;
                if (_vals[adjustedX, adjustedY] > 100) _vals[adjustedX, adjustedY] = 100;
            }
        }
        DrawHeatmap();
    }
    /// <summary>
    /// Adds a point to the heatmap, where the heatmap is normalized between 0 and a given maximum.
    /// </summary>
    /// <param name="x">X position of the point, between 0 and the given max</param>
    /// <param name="z">Z position of the point, between 0 and the given max</param>
    /// <param name="span">How many pixels each quarter of the heatmap diamond should span.</param>
    /// <param name="max">The maximum bounds of the heatmap</param>
    public void AddPointToVals(float x, float z, int span, Vector2 max)
    {
        //normalize the given point
        if (x == 0) x = 0.000001f;
        if (z == 0) x = 0.000001f;
        float calculatedX = x / max.x;
        float calculatedZ = z / max.y;
        int coordX = _vals.GetLength(0) - (int)(calculatedX * _vals.GetLength(0));
        int coordZ = _vals.GetLength(1) - (int)((1 - calculatedZ) * _vals.GetLength(1));

        float maxDistance = Vector2.Distance(new Vector2(span, span), Vector2.zero);
        for (int x1 = -span; x1 < span; x1++)
        {
            int adjustedX = coordX + x1;
            if (adjustedX >= _vals.GetLength(0)) continue;
            if (adjustedX < 0) continue;
            for (int y1 = -span; y1 < span; y1++)
            {
                int adjustedY = coordZ + y1;
                if (adjustedY >= _vals.GetLength(1)) continue;
                if (adjustedY < 0) continue;

                float addAmount = Vector2.Distance(new Vector2(x1, y1), Vector2.zero);
                addAmount = maxDistance - addAmount;
                _vals[adjustedX, adjustedY] += (int)addAmount;
                if (_vals[adjustedX, adjustedY] > 100) _vals[adjustedX, adjustedY] = 100;
            }
        }
        DrawHeatmap();
    }
    /// <summary>
    /// Draws the contents of the heatmaps positions to the texture
    /// </summary>
    public void DrawHeatmap()
    {
        int width = _vals.GetLength(0);
        int height = _vals.GetLength(1);
        Texture2D tex = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = _gradient.Evaluate((float)_vals[x, y] / 100f);
            }
        }
        tex.SetPixels(colorMap);
        tex.Apply();

        _heatmapRenderer.sharedMaterial.mainTexture = tex;

    }
    /// <summary>
    /// Clears the stored information on the heatmap and resets its texture.
    /// </summary>
    public void ClearHeatmap(float worldspaceWidth, float worldspaceHeight)
    {
        //For now this just re-initializes the heatmap.
        InitializeHeatmap(worldspaceWidth, worldspaceHeight);
    }
    private void OnCoordinateTypeUpdated(CoordinateType type)
    {
        _coordinateType = type;
    }
}
