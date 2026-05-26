using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bobail.API.Swagger;

public class GameEndpointTagOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, string> GameActionTags = new()
    {
        [nameof(Controllers.GamesController.CreateOnlineGame)] = "Online Games",
        [nameof(Controllers.GamesController.GetCurrentOnlineGame)] = "Online Games",
        [nameof(Controllers.GamesController.JoinOnlineGame)] = "Online Games",

        [nameof(Controllers.GamesController.CreateGameVsBot)] = "Bot Games",

        [nameof(Controllers.GamesController.GetHistory)] = "Game History",
        [nameof(Controllers.GamesController.GetReplay)] = "Game History",

        [nameof(Controllers.GamesController.GetUserStats)] = "Game Stats"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(Controllers.GamesController))
            return;

        var tag = GameActionTags.GetValueOrDefault(
            context.MethodInfo.Name,
            "Games");

        operation.Tags = new List<OpenApiTag>
        {
            new() { Name = tag }
        };
    }
}
