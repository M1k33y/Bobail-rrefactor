namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 586;

    public int EndgamePressureWeight { get; set; } = 1208;

    public int FriendlyAdjacencyWeight { get; set; } = 2;

    public int OpponentAdjacencyPenaltyWeight { get; set; } = 192;

    public int CenterControlWeight { get; set; } = 93;

    public int ForwardMobilityWeight { get; set; } = 294;

    public int CorridorWeight { get; set; } = 253;

    public int ImmediateWinThreatWeight { get; set; } = 15467;

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, EndgamePressure={EndgamePressureWeight}, FriendlyAdjacency={FriendlyAdjacencyWeight}, OpponentAdjacencyPenalty={OpponentAdjacencyPenaltyWeight}, CenterControl={CenterControlWeight}, ForwardMobility={ForwardMobilityWeight}, Corridor={CorridorWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}";
    }
}
