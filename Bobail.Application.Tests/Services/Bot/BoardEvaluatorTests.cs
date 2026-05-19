using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using FluentAssertions;
using Bobail.Infrastructure.Bots;

namespace Bobail.Application.Tests.Services.Bot;

public class BoardEvaluatorTests
{
    [Fact]
    public void HardBoardEvaluator_Returns_WinScore_For_Finished_Win()
    {
        var game = new Game();
        game.Finish(PlayerColor.Red);
        var evaluator = new HardBoardEvaluator(new EvaluationWeights());

        var score = evaluator.Evaluate(game, PlayerColor.Red);

        score.Should().Be(1_000_000);
    }

    [Fact]
    public void HardBoardEvaluator_Returns_LossScore_For_Finished_Loss()
    {
        var game = new Game();
        game.Finish(PlayerColor.Green);
        var evaluator = new HardBoardEvaluator(new EvaluationWeights());

        var score = evaluator.Evaluate(game, PlayerColor.Red);

        score.Should().Be(-1_000_000);
    }

    [Fact]
    public void HardBoardEvaluator_Evaluates_Player_Phase_Position()
    {
        var game = new Game();
        var evaluator = new HardBoardEvaluator(new EvaluationWeights());

        var redScore = evaluator.Evaluate(game, PlayerColor.Red);
        var greenScore = evaluator.Evaluate(game, PlayerColor.Green);

        redScore.Should().Be(-greenScore);
    }

    [Fact]
    public void HardBoardEvaluator_Evaluates_Bobail_Phase_Position()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var evaluator = new HardBoardEvaluator(new EvaluationWeights());

        var score = evaluator.Evaluate(game, PlayerColor.Green);

        score.Should().NotBe(0);
    }

    [Fact]
    public void HardBoardEvaluator_When_Bobail_Is_Missing_Throws()
    {
        var game = new Game();
        game.Board.Pieces.RemoveAll(piece => piece.IsBobail);
        var evaluator = new HardBoardEvaluator(new EvaluationWeights());

        var act = () => evaluator.Evaluate(game, PlayerColor.Red);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Bobail not found on board.");
    }

    [Fact]
    public void MediumBoardEvaluator_Uses_Same_Evaluation_As_HardBoardEvaluator()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var weights = new EvaluationWeights();
        var mediumEvaluator = new MediumBoardEvaluator(weights);
        var hardEvaluator = new HardBoardEvaluator(weights);

        var mediumScore = mediumEvaluator.Evaluate(game, PlayerColor.Green);
        var hardScore = hardEvaluator.Evaluate(game, PlayerColor.Green);

        mediumScore.Should().Be(hardScore);
    }

    [Fact]
    public void EvaluationWeights_ToString_Includes_All_Weights()
    {
        var weights = new EvaluationWeights
        {
            ProgressWeight = 1,
            PathToGoalWeight = 2,
            ImmediateWinThreatWeight = 3,
            ImmediateLossThreatWeight = 4,
            BobailMobilityWeight = 5,
            ForwardMobilityWeight = 6,
            TrapRiskWeight = 7,
            OpponentPressureWeight = 8,
            FriendlySupportWeight = 9,
            DestinationQualityWeight = 10,
            CenterControlWeight = 11,
            BehindBobailFormationWeight = 12,
            TokenDevelopmentWeight = 13
        };

        var text = weights.ToString();

        text.Should().Contain("Progress=1");
        text.Should().Contain("PathToGoal=2");
        text.Should().Contain("ImmediateWinThreat=3");
        text.Should().Contain("ImmediateLossThreat=4");
        text.Should().Contain("BobailMobility=5");
        text.Should().Contain("ForwardMobility=6");
        text.Should().Contain("TrapRisk=7");
        text.Should().Contain("OpponentPressure=8");
        text.Should().Contain("FriendlySupport=9");
        text.Should().Contain("DestinationQuality=10");
        text.Should().Contain("CenterControl=11");
        text.Should().Contain("BehindBobailFormation=12");
        text.Should().Contain("TokenDevelopment=13");
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
