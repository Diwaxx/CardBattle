using CardGame.Database;
using CardGame.Server.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL конфигурация
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");

// Добавляем поддержку JSON для Npgsql
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });

    // Включаем детализированные логи для разработки
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Применяем миграции при запуске в разработке
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        try
        {
            // Проверяем подключение к БД
            if (db.Database.CanConnect())
            {
                Console.WriteLine("? Connected to PostgreSQL successfully!");

                // Применяем миграции
                db.Database.Migrate();
                Console.WriteLine("? Database migrations applied!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Database connection failed: {ex.Message}");
            Console.WriteLine("Please check your PostgreSQL connection string in appsettings.Development.json");
            Console.WriteLine($"Connection string: {connectionString}");
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check с проверкой БД
app.MapGet("/health", async (GameDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var playersCount = await db.Players.CountAsync();

        return Results.Ok(new
        {
            status = "healthy",
            database = canConnect ? "connected" : "disconnected",
            players = playersCount,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database error",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

// Информация о сервере
app.MapGet("/", () => Results.Json(new
{
    message = "CardGame PostgreSQL Server",
    database = "PostgreSQL",
    endpoints = new
    {
        health = "/health",
        swagger = "/swagger",
        api = "/api/{controller}"
    },
    version = "1.0.0"
}));

Console.WriteLine("?? Server starting...");
Console.WriteLine($"?? Using PostgreSQL database");
Console.WriteLine($"?? Health check: /health");
Console.WriteLine($"?? API Docs: /swagger");

app.Run();