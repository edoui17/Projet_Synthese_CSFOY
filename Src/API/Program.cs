using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(p_options =>
    p_options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        p_sqlOptions => p_sqlOptions.EnableRetryOnFailure()));

// Register DI Repositories
builder.Services.AddScoped<Core.Interfaces.IPlayerRepository, Infrastructure.Repositories.PlayerRepository>();
builder.Services.AddScoped<Core.Interfaces.IInventoryRepository, Infrastructure.Repositories.InventoryRepository>();
builder.Services.AddScoped<Core.Interfaces.IStatsRepository, Infrastructure.Repositories.StatsRepository>();
builder.Services.AddScoped<Core.Interfaces.IAuthRepository, Infrastructure.Repositories.AuthRepository>();
builder.Services.AddScoped<Core.Interfaces.IConfigRepository, Infrastructure.Repositories.ConfigRepository>();

WebApplication app = builder.Build();

app.UseMiddleware<API.Middleware.ApiKeyMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
