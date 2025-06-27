using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SnakeClient.Store;
using System.Text.Json;
using System.Transactions;

namespace SnakeClient.GameClass
{
    public class GameMap : AcGameObject
    {
        public IState<PkState> PkState { get; set; }
        public Canvas2DContext? Context { get; set; }
        public BECanvas CanvasElement { get; set; }
        public ElementReference ParentElement { get; set; }
        public double L { get; set; } = 60;
        public int Rows { get; set; } = 13;
        public int Cols { get; set; } = 14;
        public List<Wall> Walls { get; set; } = new();
        public List<Snake> Snakes { get; set; } = new();

        public GameMap(Canvas2DContext context, BECanvas canvasElement, ElementReference parentElement, GameLoop gameLoop, IState<PkState> pkState): base(gameLoop)
        {
            Context = context;
            CanvasElement = canvasElement;
            ParentElement = parentElement;
            PkState = pkState;
            Snakes.Add(new Snake(0, "#4876EC", Rows - 2, 1, this, _loop));
            Snakes.Add(new Snake(1, "#F94848", 1, Cols - 2, this, _loop));
        }

        public bool CheckReady()
        {
            foreach (var snake in Snakes)
            {
                if (snake.Status != "idle") return false;
                if (snake.Direction == -1) return false;
            }
            return true;
        }

        public override async Task StartAsync()
        {
            if(PkState.Value.Map != null)
            {
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Cols; c++)
                    {
                        if (PkState.Value.Map[r, c] == 1)
                        {
                            Walls.Add(new Wall(r, c, this, _loop));
                        }
                    }
                }
            }
        }
        public async Task UpdateSizeAsync()
        {
            
        }

        public void NextStep()
        {
            foreach (var snake in Snakes)
            {
                snake.NextStep();
            }
        }

        public override async Task UpdateAsync()
        {
            //await UpdateSizeAsync();
            if (CheckReady())
            {
                NextStep();
            }
            await RenderAsync();
            
        }

        private async Task RenderAsync()
        {
            if (Context == null) return;

            // 绘制背景
            var colorEven = "#AAD751";
            var colorOdd = "#A2D149";

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    await Context.SetFillStyleAsync((r + c) % 2 == 0 ? colorEven : colorOdd);
                    await Context.FillRectAsync(c * L, r * L, L, L);
                }
            }
        }
    }
}
