#pragma once
using namespace System;

namespace SnakeCppCli {
    public ref class MapGenerator
    {
    private:
        array<int, 2>^ gameMap;
        int rows, cols, innerWallsCount;
        array<int>^ dx;
        array<int>^ dy;

        bool CheckConnect(int sx, int sy, int tx, int ty);
        bool CreateWalls();

    public:
        MapGenerator(int rows, int cols, int wallCount);
        array<int, 2>^ CreateGameMap();
    };
}