using SnakeMatchingSystem.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MatchingPool>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
// ∆Ù∂Ø∆•≈‰≥ÿ
var matchingPool = app.Services.GetRequiredService<MatchingPool>();

// ≈‰÷√HTTP«Î«Ûπ‹µ¿
app.UseRouting();
app.MapControllers();

app.Run();
