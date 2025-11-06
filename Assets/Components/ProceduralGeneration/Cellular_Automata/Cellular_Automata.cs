using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VTools.Grid;

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellular_Automata")]
public class Cellular_Automata : ProceduralGenerationMethod
{
    [SerializeField] int _density = 70;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);

        CreateWhiteNoise(_density);

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            NewGrid();
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

    }

    public void CreateWhiteNoise(float density)
    {
        for (int y = 0; y < Grid.Lenght; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int res = RandomService.Range(0, 100);

                if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                    continue;
                if (res <= density)
                {
                    AddTileToCell(cell, WATER_TILE_NAME, true);
                }
                else
                {
                    AddTileToCell(cell, GRASS_TILE_NAME, true);
                }
            }
        }
    }

    public void NewGrid()
    {
        bool[][] _newMap = new bool[Grid.Lenght][];
        for (int y = 0; y < Grid.Lenght; y++)
            _newMap[y] = new bool[Grid.Width];

        int threadCount = 4;
        int rowsPerThread = Grid.Lenght / threadCount;

        Task[] tasks = new Task[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int startY = t * rowsPerThread;
            int endY = (t == threadCount - 1) ? Grid.Lenght : startY + rowsPerThread;

            tasks[t] = Task.Run(() =>
            {
                for (int y = startY; y < endY; y++)
                {
                    if (y == 0 || y == Grid.Lenght - 1) continue;

                    for (int x = 1; x < Grid.Width - 1; x++)
                    {
                        _newMap[y][x] = CheckGround(x, y);
                    }
                }
            });
        }

        Task.WaitAll(tasks);

        for (int y = 1; y < Grid.Lenght - 1; y++)
        {
            for (int x = 1; x < Grid.Width - 1; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                    continue;

                if (_newMap[y][x])
                    AddTileToCell(cell, GRASS_TILE_NAME, true);
                else
                    AddTileToCell(cell, WATER_TILE_NAME, true);
            }
        }
    }

    public bool CheckGround(int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                if (!Grid.TryGetCellByCoordinates(x + i, y + j, out Cell cell))
                    continue;
                if (cell.GridObject.Template.Name == "Grass")
                    count++;

            }
        }
        if (count > 3)
            return true;
        return false;
    }
}

//    public void Borders()
//    {
//        for (int i = 0; i < Grid.Width; i++)
//        {
//            if (!Grid.TryGetCellByCoordinates(i, 1, out Cell cell))
//                continue;
//            if (cell.GridObject.Template.Name == "Grass")
//                AddTileToCell(cell, GRASS_TILE_NAME, true);
//        }

//    }
//}
