namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 1725; //475

    public int PathToGoalWeight { get; set; } = 501; //156

    public int ImmediateWinThreatWeight { get; set; } = 24000; //6820

    public int ImmediateLossThreatWeight { get; set; } = 14908; //17_035

    public int BobailMobilityWeight { get; set; } = 1025; //219

    public int ForwardMobilityWeight { get; set; } = 1765;//318

    public int TrapRiskWeight { get; set; } = 713;//536

    public int OpponentPressureWeight { get; set; } = 619; //217

    public int FriendlySupportWeight { get; set; } = 2287;//133

    public int DestinationQualityWeight { get; set; } = 617;//88

    public int CenterControlWeight { get; set; } = 101;//120

    public int BehindBobailFormationWeight { get; set; } = 2648;//140

    public int TokenDevelopmentWeight { get; set; } = 288;//90

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}, CenterControl={CenterControlWeight}, BehindBobailFormation={BehindBobailFormationWeight}, TokenDevelopment={TokenDevelopmentWeight}";
    }
}
