# GPC

Here you will find the basics to learn procedural content generation (Generation Procédurale de Contenu in French), with 4 algorithms.

## Table of Contents
- [1. Simple Room Placement](#1-simple-room-placement)
- [2. Binary Space Partition (BSP)](#2-binary-space-partition-bsp)
- [3. Cellular Automata](#3-cellular-automata)
- [4. Fast Noise Lite](#4-fast-noise-lite)

---

## 1. Simple Room Placement
This algorithm is about generate a field and the ntry to "brute force" the placement of rooms. We try to place a room, if we cant we try again in a random new localitation. Program stop when we place the number of room we want or a certain number of attemps.
            
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

## 3. Cellular Automata

## 4. Fast Noise Lite
