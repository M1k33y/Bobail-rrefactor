using Bobail.API.Hubs;
using Bobail.API.Middleware;
using Bobail.API.Realtime;
using Bobail.API.Swagger;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Application.Services.Bot;
using Bobail.Application.Validators;
using Bobail.Infrastructure.Bots;
using Bobail.Infrastructure.Email;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<GameEndpointTagOperationFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped<IGameRepository, SqlGameRepository>();
builder.Services.AddScoped<IGameStateRepository, SqlGameStateRepository>();
builder.Services.AddScoped<IGameHistoryRepository, SqlGameHistoryRepository>();
builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, SqlEmailVerificationTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, SqlPasswordResetTokenRepository>();
builder.Services.AddScoped<IGamePlayerRepository, SqlGamePlayerRepository>();

builder.Services.AddSingleton<InMemoryEmailSender>();
builder.Services.AddSingleton<IEmailOutbox>(sp => sp.GetRequiredService<InMemoryEmailSender>());
builder.Services.AddSingleton<SmtpEmailSender>();
builder.Services.AddSingleton<IEmailSender>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var smtpHost = configuration["Email:Smtp:Host"];

    return string.IsNullOrWhiteSpace(smtpHost)
        ? sp.GetRequiredService<InMemoryEmailSender>()
        : sp.GetRequiredService<SmtpEmailSender>();
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOnlineGameService, OnlineGameService>();
builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddSingleton<IGameLockManager, InMemoryGameLockManager>();
builder.Services.AddSingleton<IGameConnectionTracker, InMemoryGameConnectionTracker>();

builder.Services.AddSingleton<EvaluationWeights>();


builder.Services.AddScoped<MediumBoardEvaluator>();
builder.Services.AddScoped<HardBoardEvaluator>();

builder.Services.AddScoped<IBotStrategy, EasyBotStrategy>();
builder.Services.AddScoped<IBotStrategy, MediumBotStrategy>();
builder.Services.AddScoped<IBotStrategy, HardBotStrategy>();

builder.Services.AddScoped<IValidator<(string, string, string)>, RegisterValidator>();
builder.Services.AddScoped<IValidator<(string, string)>, LoginValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/game") ||
                     path.StartsWithSegments("/hubs/auth")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("1") ||
            context.User.IsInRole("Admin")));
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
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapHub<AuthHub>("/hubs/auth");
app.MapHub<GameHub>("/hubs/game");
app.MapControllers();

app.Run();

public partial class Program { }
