namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 826; //475

    public int PathToGoalWeight { get; set; } = 168; //156

    public int ImmediateWinThreatWeight { get; set; } = 13_576; //6820

    public int ImmediateLossThreatWeight { get; set; } = 6631; //17_035

    public int BobailMobilityWeight { get; set; } = 139; //219

    public int ForwardMobilityWeight { get; set; } = 289;//318

    public int TrapRiskWeight { get; set; } = 759;//536

    public int OpponentPressureWeight { get; set; } = 136; //217

    public int FriendlySupportWeight { get; set; } = 800;//133

    public int DestinationQualityWeight { get; set; } = 64;//88

    public int CenterControlWeight { get; set; } = 247;//120

    public int BehindBobailFormationWeight { get; set; } = 1148;//140

    public int TokenDevelopmentWeight { get; set; } = 117;//90

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}, CenterControl={CenterControlWeight}, BehindBobailFormation={BehindBobailFormationWeight}, TokenDevelopment={TokenDevelopmentWeight}";
    }
}
