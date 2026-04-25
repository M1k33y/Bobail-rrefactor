namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 475;

    public int PathToGoalWeight { get; set; } = 156;

    public int ImmediateWinThreatWeight { get; set; } = 6820;

    public int ImmediateLossThreatWeight { get; set; } = 17_035;

    public int BobailMobilityWeight { get; set; } = 219;

    public int ForwardMobilityWeight { get; set; } = 318;

    public int TrapRiskWeight { get; set; } = 536;

    public int OpponentPressureWeight { get; set; } = 217;

    public int FriendlySupportWeight { get; set; } = 133;

    public int DestinationQualityWeight { get; set; } = 88;

    public int CenterControlWeight { get; set; } = 120;

    public int BehindBobailFormationWeight { get; set; } = 140;

    public int TokenDevelopmentWeight { get; set; } = 90;

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}, CenterControl={CenterControlWeight}, BehindBobailFormation={BehindBobailFormationWeight}, TokenDevelopment={TokenDevelopmentWeight}";
    }
}
