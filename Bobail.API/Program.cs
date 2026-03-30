using Bobail.API.Middleware;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Application.Services.Bot;
using Bobail.Infrastructure.Bots;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IGameRepository, SqlGameRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IBotService, BotService>();

builder.Services.AddScoped<MediumBoardEvaluator>();
builder.Services.AddScoped<HardBoardEvaluator>();

builder.Services.AddScoped<IBotStrategy, EasyBotStrategy>();
builder.Services.AddScoped<IBotStrategy, MediumBotStrategy>();
builder.Services.AddScoped<IBotStrategy, HardBotStrategy>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();
