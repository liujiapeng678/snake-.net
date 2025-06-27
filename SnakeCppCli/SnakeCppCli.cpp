#include "pch.h"
#include "SnakeCppCli.h"
#include <random>

using namespace SnakeCppCli;

MapGenerator::MapGenerator(int r, int c, int wallCount)
{
    rows = r;
    cols = c;
    innerWallsCount = wallCount;
    gameMap = gcnew array<int, 2>(rows, cols);
    dx = gcnew array<int>{-1, 0, 1, 0};
    dy = gcnew array<int>{0, 1, 0, -1};
}

bool MapGenerator::CheckConnect(int sx, int sy, int tx, int ty)
{
    if (sx == tx && sy == ty) return true;

    // 检查这里是否有边界检查问题
    if (sx < 0 || sx >= rows || sy < 0 || sy >= cols) return false;
    if (gameMap[sx, sy] == 1) return false; // 已访问或是墙

    gameMap[sx, sy] = 1; // 标记为已访问

    for (int i = 0; i < 4; i++)
    {
        int x = sx + dx[i], y = sy + dy[i];
        if (x >= 0 && x < rows && y >= 0 && y < cols && gameMap[x, y] == 0)
        {
            if (CheckConnect(x, y, tx, ty))
            {
                gameMap[sx, sy] = 0; // 回溯
                return true;
            }
        }
    }
    gameMap[sx, sy] = 0; // 回溯
    return false;
}

bool MapGenerator::CreateWalls()
{
    // 清空地图
    for (int i = 0; i < rows; i++)
        for (int j = 0; j < cols; j++)
            gameMap[i, j] = 0;

    // 边界墙
    for (int r = 0; r < rows; r++)
    {
        gameMap[r, 0] = 1;
        gameMap[r, cols - 1] = 1;
    }
    for (int c = 0; c < cols; c++)
    {
        gameMap[0, c] = 1;
        gameMap[rows - 1, c] = 1;
    }

    // 随机内墙
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> rowDis(0, rows - 1);
    std::uniform_int_distribution<> colDis(0, cols - 1);

    for (int i = 0; i < innerWallsCount / 2; i++)
    {
        for (int j = 0; j < 1000; j++)
        {
            int r = rowDis(gen);
            int c = colDis(gen);

            if (gameMap[r, c] == 1 || gameMap[rows - 1 - r, cols - 1 - c] == 1)
                continue;
            if ((r == rows - 2 && c == 1) || (r == 1 && c == cols - 2))
                continue;

            gameMap[r, c] = 1;
            gameMap[rows - 1 - r, cols - 1 - c] = 1;
            break;
        }
    }

    return CheckConnect(rows - 2, 1, 1, cols - 2);
}

array<int, 2>^ MapGenerator::CreateGameMap()
{
    for (int i = 0; i < 1000; i++)
    {
        if (CreateWalls())
            break;
    }
    return gameMap;
}