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
It contais diferents cells which have diferents prefabs to build a map
### Random service
Random service is a tool for generate a random number whit a seed. We want the "Chaos" to be repeat, to test.
## 1. Simple Room Placement
This algorithm is about generate a field and try to "brute force" the placement of rooms. We try to place a room, if we cant we try again in a random new localitation. Program stop when we place the number of room we want or a certain number of attemps.
            
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
The BSP is a binary tree logic, we create a ground and then recursivly it create nods and the leaf are our room. this method is more effective tha the simple room placement and it let us connect the room smarted.


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
Here its gonna generate a white noise, whit a density of our choise. Then it will parcours the grid from the bottom left and check each cell neigboors, if a cell have 4 grounds as neirboors it will become a ground else water.
It dont check the outer borders

![ezgif-294f4588009f6539](https://github.com/user-attachments/assets/7c2eecb7-813e-4ffc-a358-59e09af0b43e)


## 4. Fast Noise Lite

