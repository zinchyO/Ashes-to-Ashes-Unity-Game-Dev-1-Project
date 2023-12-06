using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum eLevelType { snow, mountain, field, test }

public class TerrainManager : MonoBehaviour
{
    // Singleton TerrainManager
    static private TerrainManager S;

    private Tilemap map;
    private bool[] columnUpdatePotentials;  // List of columns that have been affected by a removal
    private float camY, camX;               // World coordinate size of camera
    private int groundMin;                  // Lowest y value that each column of terrain will generate to
    private float smoothingCoef = 0.4f;     // How sharp the hills should drop off (percentage)

    [Header("Inscribed")]
    public Camera cam;
    public Tile groundTile;
    public Color fieldColor = new(0.42f, 0.7f, 0.28f);
    public Color snowColor = new(1.0f, 1.0f, 1.0f);
    public Color mountainColor = new(0.4f, 0.4f, 0.4f);
    [Tooltip("Type of level of the current scene")]
    public eLevelType levelType = eLevelType.test;
    [Tooltip("Size of terrain particles. A scale of 0.1 translates to 10 particles per camera unit.")]
    public float scale = 0.1f;

    [Header("Dynamic")]
    public int terrainMaskHorizontal;
    public int terrainMaskVertical;
    public int[] terrainMask;

    private void Awake()
    {
        if (S != null)
        {
            Debug.LogError("ERROR: attempt to set TerrainManager Singleton failed");
            return;
        }
        S = this;

        levelType = TypeTransfer.TYPE;

        camY = cam.orthographicSize;
        camX = (camY * cam.aspect);

        map = GetComponentInChildren<Tilemap>();

        // Move the tilemap to the bottom left corner to ensure (0, 0) on the tilemap corresponds to (0, 0) in the terrainMask
        map.transform.position = new(-camX, -camY);
        // Scale the tilemap to the intended particle size
        map.transform.localScale = new(scale, scale, 1);
        // Make sure the tilemap is a clean slate
        map.ClearAllTiles();

        // Width and height are based on how many particles can fit on the screen (double the camera's width and height divided by the particle scale plus one)
        terrainMaskHorizontal = (int)(2 * camX / scale + 1);
        terrainMaskVertical = (int)(2 * camY / scale + 1);
        terrainMask = GenerateTerrainMask();
        columnUpdatePotentials = new bool[terrainMaskHorizontal];
        
        // Populate the tilemap based on the terrain mask generated previously
        for (int x = 0; x < terrainMaskHorizontal; x++)
        {
            columnUpdatePotentials[x] = false;
            for (int y = 0; y <= terrainMask[x]; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }
    }

    // Generates the starting terrain shape
    private int[] GenerateTerrainMask()
    {
        int[] mask = new int[terrainMaskHorizontal];
        Vector2Int[] hillApexes;

        // Establish the number of hills, color of the terrain, and the parameters of the hill shapes
        int hillCt, yLow=0, yHigh=0;
        switch (levelType)
        {
            case eLevelType.field:
                hillCt = 3;
                yLow = (int)(terrainMaskVertical * 0.4f);
                yHigh = (int)(terrainMaskVertical * 0.6f);
                smoothingCoef = 0.3f;
                groundMin = (int)(terrainMaskVertical * 0.4f);
                map.color = fieldColor;
                break;

            case eLevelType.snow:
                hillCt = 5;
                yLow = (int)(terrainMaskVertical * 0.3f);
                yHigh = (int)(terrainMaskVertical * 0.5f);
                smoothingCoef = 1.5f;
                groundMin = (int)(terrainMaskVertical * 0.3f);
                map.color = snowColor;
                break;

            case eLevelType.mountain:
                hillCt = 4;
                yLow = (int)(terrainMaskVertical * 0.3f);
                yHigh = (int)(terrainMaskVertical * 0.8f);
                smoothingCoef = 1.0f;
                groundMin = (int)(terrainMaskVertical * 0.25f);
                map.color = mountainColor;
                break;

            default:
                groundMin = (int)(terrainMaskVertical * 0.35f);
                hillCt = 0;
                map.color = Color.magenta;
                break;
        }
        hillApexes = new Vector2Int[hillCt];

        for (int hill = 0; hill < hillCt; hill++)
        {
            // Establish the horizontal range of the hill apex's possible locations (middle 4th of each zone)
            int xLow = (int)((terrainMaskHorizontal / hillCt) * (hill + 0.375f)),
                xHigh = (int)((terrainMaskHorizontal / hillCt) * (hill + 0.625f));

            // Randomly pick a point in the established ranges to place the apex
            hillApexes[hill] = new(Random.Range(xLow, xHigh), Random.Range(yLow, yHigh));
        }

        for (int x = 0; x < terrainMaskHorizontal; x++)
        {
            mask[x] = Mathf.Max(calcHeightsFromList(hillApexes, x));
        }
        return mask;
    }

    private int[] calcHeightsFromList(Vector2Int[] hillApexes, int x)
    {
        int[] heights = new int[hillApexes.Length + 1];
        heights[0] = groundMin;

        for(int i = 1; i < heights.Length; i++)
        {
            heights[i] = hillApexes[i - 1].y - (int)(Mathf.Pow((hillApexes[i - 1].x - x) * scale, 2) * smoothingCoef);
        }

        return heights;
    }

    public static float SCALE()
    {
        return S.scale;
    }

    // Takes in a world coordinate and converts it to terrain coordinates
    private static int WORLD_TO_TERRAIN_COORDINATE(float input, bool xORy)
    {
        return Mathf.FloorToInt((input + (xORy ? S.camX : S.camY)) / S.scale);
    }

    // input: world coordinate X, output: terrain height in world coordinates at X
    public static float HEIGHT_AT(float x)
    {
        float height = (S.terrainMask[Mathf.Clamp(WORLD_TO_TERRAIN_COORDINATE(x, true), 0, S.terrainMaskHorizontal-1)] + 1) * S.scale - S.camY;
        Debug.DrawLine(new(-S.camX, height, 0), new(S.camX, height, 0));
        return height;
    }

    // Accessor method to make terrain holes from other classes. values are in world space coordinates
    public static void REMOVE_FROM_TERRAIN(int radius, Vector2 pos)
    {
        S.RemoveFromTerrain(
            (int)(radius/S.scale), 
            new(WORLD_TO_TERRAIN_COORDINATE(pos.x, true), 
                WORLD_TO_TERRAIN_COORDINATE(pos.y, false))
        );
    }

    // Make a circular hole in the terrain with defined radius at position pos(x, y)
    public void RemoveFromTerrain(int radius, Vector2Int pos)
    {
        for (int x = -radius; x <= radius; x++)
        {
            int xPos = pos.x + x;
            if (0 <= xPos && xPos < terrainMaskHorizontal)
            {
                // Cache relevant values
                int terrainHeightAtX = terrainMask[xPos],
                    yOfRemovalAtX = (int)Mathf.Sqrt(radius * radius - x * x),
                    yLowerClamped = Mathf.Max(0, pos.y - yOfRemovalAtX),
                    yUpperClamped = Mathf.Min(terrainMaskVertical, pos.y + yOfRemovalAtX);

                // if the terrain height of the current X column is between the lower and upper values, set the terrain height to the lower value.
                if (yLowerClamped <= terrainHeightAtX && terrainHeightAtX <= yUpperClamped)
                    terrainMask[xPos] = Mathf.Max(0, yLowerClamped);
                // otherwise, if the terrain height is above the upper value, subtract the length of the circle that is in bounds
                else if (terrainHeightAtX > yUpperClamped)
                    terrainMask[xPos] -= yUpperClamped - yLowerClamped;

                columnUpdatePotentials[xPos] = true;
            }
        }

        UpdateColumns();
    }

    // Aggregate the particles down when there is space below
    private void UpdateColumns()
    {
        for (int i = 0; i < columnUpdatePotentials.Length; i++)
        {
            if (columnUpdatePotentials[i])
            {
                for (int y = 0; y < terrainMaskVertical; y++)
                {
                    map.SetTile(new(i, y), terrainMask[i] < y ? null : groundTile);
                }

                // Reset the current column's potential since it has been checked already
                columnUpdatePotentials[i] = false;
            }
        }
    }
}
