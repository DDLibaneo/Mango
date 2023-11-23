using Mango.Services.RewardsAPI.Data;
using Mango.Services.RewardsAPI.Messaging;
using Mango.Services.RewardsAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var optionBuilder = AddDatabaseServices(builder);

builder.Services.AddSingleton(new RewardsService(optionBuilder.Options));

builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();

void ApplyMigration()
{
    using var scope = app.Services.CreateScope();
    var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (_db.Database.GetPendingMigrations().Any())
    {
        _db.Database.Migrate();
    }
}

static DbContextOptionsBuilder<AppDbContext> AddDatabaseServices(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<AppDbContext>(option =>
    {
        option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    return optionBuilder;
}