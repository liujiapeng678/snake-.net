using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace SnakeClient.GameClass
{
    // 游戏基类
    public abstract class AcGameObject
    {
        public GameLoop _loop;
        public bool hasCalledStart { get; set; } = false;
        public double timeDelta { get; set; } = 0;

        public AcGameObject(GameLoop loop)
        {
            hasCalledStart = false;
            timeDelta = 0;
            _loop = loop;
            _loop.gameObjects.Add(this);
        }
        public virtual async Task StartAsync() { }
        public virtual async Task UpdateAsync() { }
        public virtual async Task OnDestroyAsync() { }
        public async Task DestroyAsync()
        {
            await OnDestroyAsync();
            _loop.gameObjects.Remove(this);
        }
    }
}
