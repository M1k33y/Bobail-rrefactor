namespace Bobail.Application.DTOs;

public class ActiveOnlineGameResponse
{
    public bool HasActiveGame => GameId.HasValue;

    public Guid? GameId { get; set; }
}
