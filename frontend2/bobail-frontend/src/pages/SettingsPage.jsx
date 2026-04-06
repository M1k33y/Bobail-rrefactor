import { useTheme } from "../app/theme/ThemeContext";
import "../styles/SettingsPage.css";
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
        <div>
            <h1>Settings</h1>

            <h2>App Theme</h2>
            <div className="theme-grid">
                {["dark", "light", "ocean"].map(theme => (
                    <div
                        key={theme}
                        className={`theme-card ${appTheme === theme ? "active" : ""}`}
                        onClick={() => setAppTheme(theme)}
                    >
                        <div className={`preview preview-${theme}`} />
                        <span>{theme}</span>
                    </div>
                ))}
            </div>

            <h2>Board Theme</h2>
            <div className="theme-grid">
                {["classic", "wood", "blue"].map(theme => (
                    <div
                        key={theme}
                        className={`theme-card ${boardTheme === theme ? "active" : ""}`}
                        onClick={() => setBoardTheme(theme)}
                    >
                        <div className={`preview preview-board-${theme}`} />
                        <span>{theme}</span>
                    </div>
                ))}
            </div>

            <h2>Piece Style</h2>
            <div className="theme-grid">
                {["default", "flat", "neon"].map(theme => (
                    <div
                        key={theme}
                        data-piece={theme}
                        className={`theme-card ${pieceTheme === theme ? "active" : ""}`}
                        onClick={() => setPieceTheme(theme)}
                    >
                        <div className="preview piece-preview" />
                        <span>{theme}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default SettingsPage;