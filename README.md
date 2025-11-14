# GPC

Here you will find the basics to learn procedural content generation (Generation Procédurale de Contenu in French), with 4 algorithms.

## Table of Contents
- [1. Simple Room Placement](#1-simple-room-placement)
- [2. Binary Space Partition (BSP)](#2-binary-space-partition-bsp)
- [3. Cellular Automata](#3-cellular-automata)
- [4. Fast Noise Lite](#4-fast-noise-lite)

---
## Tools
In this project we will use differents tools 
### ProceduralGridGenerator
This algo will create a grid in fonction of a generation algorithm
### Grid 
It contais diferents cells which have diferents prefabs to build a map. This cells are gameObjects
### Random service
Random service is a tool for generate a random number whit a seed. We want the "Chaos" to be repeat, to test.
## 1. Simple Room Placement
This algorithm generates a field and tries to “brute-force” the placement of rooms. It attempts to place a room; if it cannot, it tries again in a random location. The program stops when the desired number of rooms is placed or a maximum number of attempts is reached.          
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

## 2. Binary Space Partition (BSP)
BSP is a binary tree logic. We create a ground, then recursively divide it into nodes. The leaves are our rooms. This method is more efficient than simple room placement and allows us to connect rooms more intelligently.

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
## 3. Cellular Automata
This algorithm generates white noise with a chosen density. It then goes through the grid from bottom-left and checks each cell’s neighbors. If a cell has 4 ground neighbors, it becomes ground; otherwise, it becomes water. The outer borders are not checked.

![ezgif-294f4588009f6539](https://github.com/user-attachments/assets/7c2eecb7-813e-4ffc-a358-59e09af0b43e)


## 4. Fast Noise Lite
This algorithm generates noise as a grid of floats in the range (-1, 1). The values represent height. Heights are gradual: a cell with height -1 cannot have a neighbor with height 1. My contribution was adding an interpretation of the heights (e.g., below 0 is water, above 0.5 is mountain). This algorithm can use different types of noise, and the goal is to tweak parameters to get the desired result.

<img width="453" height="606" alt="Capture d&#39;écran 2025-11-14 144558" src="https://github.com/user-attachments/assets/ed454437-b0eb-4165-9ad9-17a8aef47736" />


## Credits
I want to thank RUTKOWSKI Yona for the lessons and the code of the tools used in this project.
Thanks also to FastNoise: https://github.com/Auburn/FastNoiseLite
