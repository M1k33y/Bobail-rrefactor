namespace Bobail.Application.Services.Bot;

public class EvaluationWeights
{
    public int ProgressWeight { get; set; } = 436; //475

    public int PathToGoalWeight { get; set; } = 272; //156

    public int ImmediateWinThreatWeight { get; set; } = 12_789; //6820

    public int ImmediateLossThreatWeight { get; set; } = 21_450; //17_035

    public int BobailMobilityWeight { get; set; } = 1260; //219

    public int ForwardMobilityWeight { get; set; } = 493;//318

    public int TrapRiskWeight { get; set; } = 547;//536

    public int OpponentPressureWeight { get; set; } = 187; //217

    public int FriendlySupportWeight { get; set; } = 443;//133

    public int DestinationQualityWeight { get; set; } = 280;//88

    public int CenterControlWeight { get; set; } = 600;//120

    public int BehindBobailFormationWeight { get; set; } = 1173;//140

    public int TokenDevelopmentWeight { get; set; } = 73;//90

    public override string ToString()
    {
        return $"Progress={ProgressWeight}, PathToGoal={PathToGoalWeight}, ImmediateWinThreat={ImmediateWinThreatWeight}, ImmediateLossThreat={ImmediateLossThreatWeight}, BobailMobility={BobailMobilityWeight}, ForwardMobility={ForwardMobilityWeight}, TrapRisk={TrapRiskWeight}, OpponentPressure={OpponentPressureWeight}, FriendlySupport={FriendlySupportWeight}, DestinationQuality={DestinationQualityWeight}, CenterControl={CenterControlWeight}, BehindBobailFormation={BehindBobailFormationWeight}, TokenDevelopment={TokenDevelopmentWeight}";
    }
}
