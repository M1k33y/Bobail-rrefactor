using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Services;

public interface IBotService
{
    Task ExecuteSingleMoveAsync(Game game);
}