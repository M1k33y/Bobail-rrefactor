namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 520;

    public int PathToGoalWeight { get; set; } = 430;

    public int ImmediateWinThreatWeight { get; set; } = 12_000;

    public int ImmediateLossThreatWeight { get; set; } = 14_000;

    public int BobailMobilityWeight { get; set; } = 120;

    public int ForwardMobilityWeight { get; set; } = 220;

    public int TrapRiskWeight { get; set; } = 360;

    public int OpponentPressureWeight { get; set; } = 180;

    public int FriendlySupportWeight { get; set; } = 90;

    public int DestinationQualityWeight { get; set; } = 150;

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}";
    }
}
