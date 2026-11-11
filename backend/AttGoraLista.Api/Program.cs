using System.Text.Json.Serialization;
using AttGoraLista.Api.Data;
using AttGoraLista.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Enum-värden skickas som tydlig text, exempelvis "STARTAD", i stället för heltal.
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Anslutningen kan sättas i appsettings eller med ConnectionStrings__DefaultConnection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Databasanslutningen 'DefaultConnection' saknas.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Scoped innebär en serviceinstans per HTTP-anrop, samma livslängd som DbContext.
builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.MapControllers();

// Skapa databasen och lägg in exempeldata. För produktion rekommenderas MigrateAsync().
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}

app.Run();

// Gör Program åtkomlig för framtida WebApplicationFactory-baserade integrationstester.
public partial class Program { }
