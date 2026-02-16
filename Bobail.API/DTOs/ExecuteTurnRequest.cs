public class ExecuteTurnRequest
{
    public MoveDto? BobailMove { get; set; }
    public MoveDto PlayerMove { get; set; } = default!;
}

public class MoveDto
{
    public int FromRow { get; set; }
    public int FromColumn { get; set; }
    public int ToRow { get; set; }
    public int ToColumn { get; set; }
}
