using Bobail.Domain.Games;

namespace Bobail.Training.Simulation;

public sealed record GameResult(PlayerColor? Winner, int Turns, bool ReachedTurnLimit);
