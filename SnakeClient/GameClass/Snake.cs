using Microsoft.JSInterop;
using System.Text.Json;

namespace SnakeClient.GameClass
{
    public class Snake : AcGameObject
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public GameMap GameMap { get; set; }
        public List<Cell> Cells { get; set; }
        public double Speed { get; set; } = 0.1;
        public int Direction { get; set; } = -1; // 0上 1右 2下 3左
        public string Status { get; set; } = "idle"; // idle, move, die
        public Cell? NextCell { get; set; }
        public int Step { get; set; } = 0;
        public int EyeDirection { get; set; }

        private readonly int[] dr = { -1, 0, 1, 0 };
        private readonly int[] dc = { 0, 1, 0, -1 };
        private readonly double eps = 1e-2;

        // 眼睛位置偏移
        private readonly double[,] eyeDx = new double[,] { { -1, 1 }, { 1, 1 }, { 1, -1 }, { -1, -1 } };
        private readonly double[,] eyeDy = new double[,] { { -1, -1 }, { -1, 1 }, { 1, 1 }, { 1, -1 } };

        public Snake(int id, string color, int r, int c, GameMap gameMap, GameLoop gameLoop) : base(gameLoop)
        {
            Id = id;
            Color = color;
            GameMap = gameMap;
            Cells = new List<Cell> { new Cell(r, c) };
            EyeDirection = id == 1 ? 2 : 0;
        }

        public bool CheckTailIncreasing()
        {
            return Step % 3 == 1;
        }

        public void SetDirection(int d)
        {
            Direction = d;
        }

        public void NextStep()
        {
            var d = Direction;
            NextCell = new Cell(Cells[0].R + dr[d], Cells[0].C + dc[d]);
            EyeDirection = d;
            Direction = -1;
            Status = "move";
            Step++;

            // 完全模拟JavaScript的逻辑
            var len = Cells.Count;
            Cells.Add(default!); // 添加占位元素，扩展 List 长度

            for (int i = len; i > 0; i--)
            {
                Cells[i] = DeepClone(Cells[i - 1]);
            }
        }

        public static T DeepClone<T>(T obj)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
        }

        public override async Task UpdateAsync()
        {
            if (Status == "move")
            {
                UpdateMove();
            }
            await RenderAsync();
            Console.WriteLine(Cells.Count.ToString());
        }

        private void UpdateMove()
        {
            if (NextCell == null) return;

            var dx = NextCell.X - Cells[0].X;
            var dy = NextCell.Y - Cells[0].Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < eps)
            {
                //Console.WriteLine("++++++++++++++移动结束+++++++++++++++++");
                Cells[0] = NextCell;
                NextCell = null;
                Status = "idle";

                if (!CheckTailIncreasing())
                {
                    Cells.RemoveAt(Cells.Count - 1);
                }
            }
            else
            {
                //Console.WriteLine("===============正在移动===================");
                var moveDistance = Speed * timeDelta / 1000;
                //Console.WriteLine($"==============={moveDistance}===================");
                //Console.WriteLine($"==============={distance}===================");
                Cells[0].X += moveDistance * dx / distance;
                Cells[0].Y += moveDistance * dy / distance;

                if (!CheckTailIncreasing())
                {
                    var len = Cells.Count;
                    var tail = Cells[len - 1];
                    var tailTarget = Cells[len - 2];
                    var tailDx = tailTarget.X - tail.X;
                    var tailDy = tailTarget.Y - tail.Y;
                    var tailDistance = Math.Sqrt(tailDx * tailDx + tailDy * tailDy);
                    if (tailDistance > eps)
                    {
                        tail.X += moveDistance * tailDx / tailDistance;
                        tail.Y += moveDistance * tailDy / tailDistance;
                    }
                }
            }
        }

        private async Task RenderAsync()
        {
            var L = GameMap.L;
            var ctx = GameMap.Context;

            if (ctx == null) return;

            // 设置蛇身颜色
            await ctx.SetFillStyleAsync(Status == "die" ? "white" : Color);

            // 绘制蛇身圆圈
            foreach (var cell in Cells)
            {
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(cell.X * L, cell.Y * L, L / 2 * 0.8, 0, Math.PI * 2);
                await ctx.FillAsync();
            }

            // 绘制蛇身连接部分
            for (int i = 1; i < Cells.Count; i++)
            {
                var a = Cells[i - 1];
                var b = Cells[i];

                if (Math.Abs(a.X - b.X) < eps && Math.Abs(a.Y - b.Y) < eps)
                    continue;
                else if (Math.Abs(a.X - b.X) < eps)
                    await ctx.FillRectAsync((a.X - 0.4) * L, Math.Min(a.Y, b.Y) * L, L * 0.8, Math.Abs(a.Y - b.Y) * L);
                else
                    await ctx.FillRectAsync(Math.Min(a.X, b.X) * L, (a.Y - 0.4) * L, Math.Abs(a.X - b.X) * L, L * 0.8);
            }

            // 绘制眼睛
            await ctx.SetFillStyleAsync("black");
            for (int i = 0; i < 2; i++)
            {
                var eyeX = (Cells[0].X + eyeDx[EyeDirection, i] * 0.15) * L;
                var eyeY = (Cells[0].Y + eyeDy[EyeDirection, i] * 0.15) * L;
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(eyeX, eyeY, L * 0.065, 0, Math.PI * 2);
                await ctx.FillAsync();
            }
        }
    }
}
