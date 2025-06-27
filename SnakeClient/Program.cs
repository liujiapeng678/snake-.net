using Blazor.Extensions;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SnakeClient;
using SnakeClient.Services;
using SnakeClient.GameClass;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 添加 Fluxor
builder.Services.AddFluxor(options => { options.ScanAssemblies(typeof(Program).Assembly); });

// 注册WebSocket服务为单例（在整个应用生命周期中共享同一个实例）
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddScoped<BECanvasComponent>();
builder.Services.AddSingleton<GameLoop>();
var app = builder.Build();

await app.RunAsync();
