import { useEffect, useState } from "react";
import { CalendarDays, CircleOff, Trophy, Users } from "lucide-react";
import { gameApi } from "../../game/api/gameApi";
import { getStoredNickname } from "../../auth/utils/authStorage";
import "../styles/GameStatsPage.css";

function formatMemberSince(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
  }).format(new Date(value));
}

function StatCard({ icon: Icon, label, value, tone = "default", helper }) {
  return (
    <article className={`stats-card stats-card-${tone}`}>
      <div className="stats-card-icon">
        <Icon size={20} />
      </div>
      <div>
        <p className="stats-card-label">{label}</p>
        <h2 className="stats-card-value">{value}</h2>
        {helper ? <p className="stats-card-helper">{helper}</p> : null}
      </div>
    </article>
  );
}

function ColorBreakdownItem({ label, value, accent }) {
  return (
    <div className="stats-breakdown-item">
      <div className="stats-breakdown-copy">
        <span className={`stats-breakdown-dot ${accent}`} />
        <span>{label}</span>
      </div>
      <strong>{value}</strong>
    </div>
  );
}

function GameStatsPage() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const nickname = getStoredNickname();

  useEffect(() => {
    let active = true;

    async function loadStats() {
      try {
        setLoading(true);
        const data = await gameApi.getUserStats();

        if (!active) {
          return;
        }

        setStats(data);
        setError("");
      } catch (err) {
        if (active) {
          setError(err.message || "Failed to load game stats.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadStats();

    return () => {
      active = false;
    };
  }, []);

  const hasGames = (stats?.totalGamesPlayed ?? 0) > 0;
  const displayName = nickname || "Player";

  return (
    <div className="game-stats-page">
      <section className="game-stats-hero">
        <p className="game-stats-eyebrow">GameStats</p>
        <div className="game-stats-hero-row">
          <div>
            <h1>{displayName}'s performance</h1>
            
          </div>
        </div>
      </section>

      {loading && <div className="game-stats-state">Loading your stats...</div>}

      {!loading && error && <div className="game-stats-error">{error}</div>}

      {!loading && !error && stats && (
        <>
          <section className="game-stats-grid">
            <StatCard
              icon={Users}
              label="Total Games Played"
              value={stats.totalGamesPlayed}
              helper="Finished matches"
            />
            <StatCard
              icon={Trophy}
              label="Total Wins"
              value={stats.totalWins}
              tone="success"
              helper={hasGames ? `${Math.round((stats.totalWins / stats.totalGamesPlayed) * 100)}% win rate` : "No games yet"}
            />
            <StatCard
              icon={CircleOff}
              label="Total Losses"
              value={stats.totalLosses}
              tone="danger"
              helper={hasGames ? `${Math.round((stats.totalLosses / stats.totalGamesPlayed) * 100)}% loss rate` : "No games yet"}
            />
            <StatCard
              icon={CalendarDays}
              label="Member Since"
              value={formatMemberSince(stats.memberSince)}
              helper="Account creation date"
            />
          </section>

          {!hasGames && (
            <div className="game-stats-state">
              No completed games yet. Finish a match and your personal stats will show up here.
            </div>
          )}

          {hasGames && (
            <section className="stats-breakdown-panel">
              <div className="stats-breakdown-header">
                <div>
                  <p className="game-stats-eyebrow">Color Breakdown</p>
                  <h2>Results by side</h2>
                </div>
                
              </div>

              <div className="stats-breakdown-grid">
                <div className="stats-breakdown-card">
                  <h3>Wins</h3>
                  <ColorBreakdownItem label="Wins with Green" value={stats.winsWithGreen} accent="green" />
                  <ColorBreakdownItem label="Wins with Red" value={stats.winsWithRed} accent="red" />
                </div>

                <div className="stats-breakdown-card">
                  <h3>Losses</h3>
                  <ColorBreakdownItem label="Losses with Green" value={stats.lossesWithGreen} accent="green" />
                  <ColorBreakdownItem label="Losses with Red" value={stats.lossesWithRed} accent="red" />
                </div>
              </div>
            </section>
          )}
        </>
      )}
    </div>
  );
}

export default GameStatsPage;
