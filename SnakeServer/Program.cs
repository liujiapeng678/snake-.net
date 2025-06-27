using SnakeServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 注册HttpClient服务
builder.Services.AddHttpClient();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
// 启用WebSocket支持
app.UseWebSockets();

// 使用WebSocket中间件
app.UseMiddleware<WebSocketMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");
app.Run();

