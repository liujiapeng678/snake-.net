using Microsoft.JSInterop;

namespace SnakeClient.GameClass
{
    public class Wall : AcGameObject
    {
        public int R { get; set; }
        public int C { get; set; }
        public GameMap GameMap { get; set; }
        public string Color { get; set; } = "#B37226";

        public Wall(int r, int c, GameMap gameMap, GameLoop gameLoop) : base(gameLoop)
        {
            R = r;
            C = c;
            GameMap = gameMap;
        }

        public override async Task UpdateAsync()
        {
            await RenderAsync();
        }

        private async Task RenderAsync()
        {
            var L = GameMap.L;
            var ctx = GameMap.Context;

            if (ctx == null) return;

            await ctx.SetFillStyleAsync(Color);
            await ctx.FillRectAsync(C * L, R * L, L, L);
        }
    }
}
