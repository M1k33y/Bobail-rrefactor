import { useTheme } from "../../../app/theme/ThemeContext";
import "../styles/SettingsPage.css";

const appThemeOptions = [
  { id: "dark", label: "Night Ink", description: "High contrast dark default." },
  { id: "light", label: "Soft Sky", description: "Cool light workspace." },
  { id: "forest", label: "Forest Study", description: "Calm green reading palette." },
  { id: "ember", label: "Ember Desk", description: "Warm dusk tones with strong accents." },
  { id: "plum", label: "Plum Room", description: "Rich violet-neutral contrast." },
  { id: "paper", label: "Paper Library", description: "Warm cream background for long sessions." }
];

const boardThemeOptions = [
  { id: "classic", label: "Classic", description: "Graphite tournament board." },
  { id: "wood", label: "Wood", description: "Warm carved table feel." },
  { id: "royal", label: "Royal", description: "Deep navy strategy board." },
  { id: "moss", label: "Moss", description: "Earthy green study board." },
  { id: "sunset", label: "Sunset", description: "Orange-rose dramatic contrast." },
  { id: "marble", label: "Marble", description: "Light stone minimalist board." }
];

const pieceThemeOptions = [
  { id: "gloss", label: "Gloss", description: "Rounded, polished counters." },
  { id: "flat", label: "Flat", description: "Clean matte colors." },
  { id: "neon", label: "Neon", description: "Arcade-style bright glow." },
  { id: "pastel", label: "Pastel", description: "Soft candy tones." },
  { id: "obsidian", label: "Obsidian", description: "Darker dramatic gradients." },
  { id: "candy", label: "Candy", description: "Playful saturated palette." }
];

function SettingsPage() {
  const {
    appTheme,
    setAppTheme,
    boardTheme,
    setBoardTheme,
    pieceTheme,
    setPieceTheme
  } = useTheme();

  return (
    <div className="settings-page">
      <div className="settings-header">
        <p className="settings-eyebrow">Appearance</p>
        
        <p className="settings-subtitle">
          Choose an app palette with stronger contrast, then pair it with your preferred board and piece style.
        </p>
      </div>

      <h2>App Theme</h2>
      <div className="theme-grid">
        {appThemeOptions.map((theme) => (
          <div
            key={theme.id}
            className={`theme-card ${appTheme === theme.id ? "active" : ""}`}
            onClick={() => setAppTheme(theme.id)}
          >
            <div className={`preview preview-${theme.id}`} />
            <span>{theme.label}</span>
            <small>{theme.description}</small>
          </div>
        ))}
      </div>

      <h2>Board Theme</h2>
      <div className="theme-grid">
        {boardThemeOptions.map((theme) => (
          <div
            key={theme.id}
            className={`theme-card ${boardTheme === theme.id ? "active" : ""}`}
            onClick={() => setBoardTheme(theme.id)}
          >
            <div className={`preview preview-board-${theme.id}`} />
            <span>{theme.label}</span>
            <small>{theme.description}</small>
          </div>
        ))}
      </div>

      <h2>Piece Style</h2>
      <div className="theme-grid">
        {pieceThemeOptions.map((theme) => (
          <div
            key={theme.id}
            data-piece={theme.id}
            className={`theme-card ${pieceTheme === theme.id ? "active" : ""}`}
            onClick={() => setPieceTheme(theme.id)}
          >
            <div className="preview piece-preview" />
            <span>{theme.label}</span>
            <small>{theme.description}</small>
          </div>
        ))}
      </div>
    </div>
  );
}

export default SettingsPage;
