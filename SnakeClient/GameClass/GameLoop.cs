using Microsoft.JSInterop;

namespace SnakeClient.GameClass
{
    public class GameLoop
    {
        private readonly IJSRuntime _jsRuntime;
        public readonly List<AcGameObject> gameObjects = new List<AcGameObject>();
        private double lastTimestamp = 0;
        private DotNetObjectReference<GameLoop> _objRef;

        public GameLoop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _objRef = DotNetObjectReference.Create(this);
        }
        
        
        // 启动游戏循环
        public async Task StartGameLoop()
        {
            await _jsRuntime.InvokeVoidAsync("startGameLoop", _objRef);
        }

        // 被JavaScript调用的Step方法
        [JSInvokable]
        public async Task Step(double timestamp)
        {
            // 创建集合的副本来避免并发修改异常
            var gameObjectsCopy = gameObjects.ToArray();
            foreach (var obj in gameObjectsCopy)
            {
                // 检查对象是否仍在原集合中（防止在异步操作期间被移除）
                if (!gameObjects.Contains(obj)) continue;

                if (!obj.hasCalledStart)
                {
                    //Console.WriteLine($"{obj.ToString()} Start");
                    obj.hasCalledStart = true;
                    await obj.StartAsync();
                }
                else
                {
                    //Console.WriteLine($"{obj.ToString()} Update");
                    await obj.UpdateAsync();
                    obj.timeDelta = timestamp - lastTimestamp;
                    //Console.WriteLine(obj.ToString());
                    //Console.WriteLine(obj.timeDelta);
                }
            }
            lastTimestamp = timestamp;
        }

    }
}
