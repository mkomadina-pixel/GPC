using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using VTools.Grid;
using static FastNoiseLite;

[CreateAssetMenu(menuName = "Procedural Generation Method/Fast noise")]
public class FastNoise : ProceduralGenerationMethod
{
    [Header("Height param")]
    [SerializeField, Range(-1, 1f)]
    public float _waterHeigth = -0.5f;
    [SerializeField, Range(-1, 1f)]
    public float _sandHeigth = 0f;
    [SerializeField, Range(-1, 1f)]
    public float _grassHeigth = 0.5f;
    [SerializeField, Range(-1, 1f)]
    public float _rockHeigth = 1f;

    [Header("Noise param")]
    [SerializeField, Range(0, 0.2f)]
    public float _frequency = 0.1f;
    [SerializeField, Range(0.5f, 1.5f)]
    public float _amplitude = 1.0f;

    [SerializeField]
    public FastNoiseLite.FractalType _fractalType = FractalType.None;
    [SerializeField, Range(0, 10f)]
    public float _lacunarity = 2;
    [SerializeField, Range(0, 1)]
    public float _persistance = 0.5f;




    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        float[,] noiseData = GetNoise();

        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                    continue;
                AddTileToCell(cell, GetTile(noiseData[x, y]), true);
            }
        }
        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
    }

    private float[,] GetNoise()
    {
        // Create and configure FastNoise object
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(_frequency);
        noise.SetFractalType(_fractalType);
        noise.SetFractalLacunarity(_lacunarity);
        noise.SetFractalGain(_persistance);

        float[,] noiseData = new float[128, 128];

        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                noiseData[x, y] = _amplitude * noise.GetNoise(x, y);
            }
        }
        return noiseData;
    }



    private string GetTile(float tile)
    {
        string res;
        if (tile < _waterHeigth)
            res = "Water";
        else if (tile < _sandHeigth)
            res = "Sand";
        else if (tile < _grassHeigth)
            res = "Grass";
        else
            res = "Rock";

        return res;
    }
    
}
