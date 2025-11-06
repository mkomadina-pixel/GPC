using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private int _roomsMaxWidth = 5;
        [SerializeField] private int _roomsMinWidth = 2;
        [SerializeField] private int _roomsMaxHeight = 5;
        [SerializeField] private int _roomsMinHeight = 2;

        [NonSerialized] List<RectInt> Rooms;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            // ........
            Rooms = new List<RectInt>();

            for (int i = 0; i < _maxSteps && i < _maxRooms; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Your algorithm here

                int width = RandomService.Range(_roomsMinWidth, _roomsMaxWidth);
                int height = RandomService.Range(_roomsMinHeight, _roomsMaxHeight);

                int x = RandomService.Range(0, Grid.Width - width);
                int z = RandomService.Range(0, Grid.Lenght - height);

                RectInt room = new RectInt(x, z, width, height);

                if (!CanPlaceRoom(room, 1))
                {
                    i--;
                    continue;
                }
                PlaceRoom(room);
                Rooms.Add(room);
                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            
            // Final ground building.
            BuildGround();

            Rooms.Sort( (a, b) => a.x.CompareTo(b.x) );

   

            for (int i = 0; i < Rooms.Count - 1; i++)
            {
                FindPath(Rooms[i], Rooms[i + 1]);
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

        private void FindPath(RectInt room1, RectInt room2)
        {
            Vector2 center1 = room1.GetCenter();
            Vector2 center2 = room2.GetCenter();

            DrawPath(center1, center2);

        }

        private void DrawPath(Vector2 center1, Vector2 center2)
        {
            for (int x = (int)center1.x; x <= center2.x; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, (int)center1.y, out Cell cell))
                    continue;
                if (cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);

            }

            if (center2.y < center1.y)
            {
                float temp = center2.y;
                center2.y = center1.y;
                center1.y = temp;
            }

            for (int y = (int)center1.y; y <= center2.y; y++)
            {
                if (!Grid.TryGetCellByCoordinates((int)center2.x, y, out Cell cell))
                    continue;
                if(cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
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
    }
}