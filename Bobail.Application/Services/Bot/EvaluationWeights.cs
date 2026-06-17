namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 1588; //475

    public int PathToGoalWeight { get; set; } = 691; //156

    public int ImmediateWinThreatWeight { get; set; } = 21016; //6820

    public int ImmediateLossThreatWeight { get; set; } = 8618; //17_035

    public int BobailMobilityWeight { get; set; } = 1046; //219

    public int ForwardMobilityWeight { get; set; } = 1748;//318

    public int TrapRiskWeight { get; set; } = 1337;//536

    public int OpponentPressureWeight { get; set; } = 947; //217

    public int FriendlySupportWeight { get; set; } = 2121;//133

    public int DestinationQualityWeight { get; set; } = 393;//88

    public int CenterControlWeight { get; set; } = 238;//120

    public int BehindBobailFormationWeight { get; set; } = 3225;//140

    public int TokenDevelopmentWeight { get; set; } = 447;//90

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}, CenterControl={CenterControlWeight}, BehindBobailFormation={BehindBobailFormationWeight}, TokenDevelopment={TokenDevelopmentWeight}";
    }
}
