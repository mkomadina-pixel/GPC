using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]

public class BSP : ProceduralGenerationMethod
{
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var allGrid = new RectInt(0,0, Grid.Width, Grid.Lenght);
        BSPNod root = new BSPNod(allGrid, 0);
        BuildGround();
        DrawRooms(root);
        ConectSisters(root);

    }

    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

        // Instantiate ground blocks
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }

                GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
            }
        }
    }

    private void DrawRooms(BSPNod room)
    {
        if (room._child1 != null)
        {
            DrawRooms(room._child1);
            DrawRooms(room._child2);
        }
        else
        {
            PlaceRoom(room._bounds);
        }
    }

    private void PlaceRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                    continue;
                AddTileToCell(cell, ROOM_TILE_NAME, true);

            }
        }

    }

    private void ConectSisters(BSPNod room)
    {
        if (room._child1 != null)
        {
            ConectSisters(room._child1);
            ConectSisters(room._child2);

            Vector2Int c1 = room._child1._bounds.GetCenter();
            Vector2Int c2 = room._child2._bounds.GetCenter();

            int fixedY = c1.y;
            for (int x = Mathf.Min(c1.x, c2.x); x <= Mathf.Max(c1.x, c2.x); x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, fixedY, out Cell cell))
                    continue;
                if (cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }

            // Couloir vertical
            int fixedX = c2.x;
            for (int y = Mathf.Min(c1.y, c2.y); y <= Mathf.Max(c1.y, c2.y); y++)
            {
                if (!Grid.TryGetCellByCoordinates(fixedX, y, out Cell cell))
                    continue;
                if (cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }
    }

}

public class BSPNod
{
    public RectInt _bounds;
    public BSPNod _child1, _child2;
    int min_width_grid = 5, min_height_grid = 5;
    public int spacing = 2;

    public BSPNod(RectInt bounds, int step)
    {
        _bounds = bounds;

        if (step < 16)
        {
            RectInt b1 = new RectInt(), b2 = new RectInt();

            // Alternate splits for better balance
            bool splitVertical = (step % 2 == 0);

            if (splitVertical && bounds.width > min_width_grid * 2)
            {
                int minSplit = bounds.x + Mathf.RoundToInt(bounds.width * 0.35f);
                int maxSplit = bounds.x + Mathf.RoundToInt(bounds.width * 0.65f);
                int split = Random.Range(minSplit, maxSplit);

                b1 = new RectInt(bounds.x + spacing, bounds.y, split - bounds.x - spacing, bounds.height);
                b2 = new RectInt(split + spacing, bounds.y, bounds.xMax - split - spacing, bounds.height);
            }
            else if (!splitVertical && bounds.height > min_height_grid * 2)
            {
                int minSplit = bounds.y + Mathf.RoundToInt(bounds.height * 0.35f);
                int maxSplit = bounds.y + Mathf.RoundToInt(bounds.height * 0.65f);
                int split = Random.Range(minSplit, maxSplit);

                b1 = new RectInt(bounds.x, bounds.y + spacing, bounds.width, split - bounds.y - spacing);
                b2 = new RectInt(bounds.x, split + spacing, bounds.width, bounds.yMax - split - spacing);
            }
            else
            {
                return;
            }

            _child1 = new BSPNod(b1, step + 1);
            _child2 = new BSPNod(b2, step + 1);
        }
    }

}
