import { createContext, useContext, useEffect, useState } from "react";

const ThemeContext = createContext();

const appThemes = {
  dark: {
    background: "#111827",
    text: "#e5e7eb",
    sidebar: "#1f2937",
    surface: "rgba(255, 255, 255, 0.05)",
    surfaceStrong: "rgba(255, 255, 255, 0.08)",
    surfaceSoft: "rgba(255, 255, 255, 0.03)",
    border: "rgba(255, 255, 255, 0.1)",
    mutedText: "rgba(229, 231, 235, 0.72)",
    accent: "#60a5fa",
    accentStrong: "#3b82f6",
    success: "#4ade80",
    danger: "#f87171",
    shadow: "rgba(0, 0, 0, 0.35)"
  },
  light: {
    background: "#dbe8f0",
    text: "#162230",
    sidebar: "#edf3f7",
    surface: "rgba(255, 255, 255, 0.75)",
    surfaceStrong: "rgba(255, 255, 255, 0.92)",
    surfaceSoft: "rgba(255, 255, 255, 0.56)",
    border: "rgba(22, 34, 48, 0.12)",
    mutedText: "rgba(22, 34, 48, 0.72)",
    accent: "#2563eb",
    accentStrong: "#1d4ed8",
    success: "#15803d",
    danger: "#b91c1c",
    shadow: "rgba(44, 62, 80, 0.12)"
  },
  forest: {
    background: "#0f1a17",
    text: "#edf6ef",
    sidebar: "#182521",
    surface: "rgba(255, 255, 255, 0.05)",
    surfaceStrong: "rgba(28, 42, 37, 0.92)",
    surfaceSoft: "rgba(255, 255, 255, 0.03)",
    border: "rgba(167, 243, 208, 0.12)",
    mutedText: "rgba(237, 246, 239, 0.72)",
    accent: "#34d399",
    accentStrong: "#10b981",
    success: "#22c55e",
    danger: "#f87171",
    shadow: "rgba(0, 0, 0, 0.34)"
  },
  ember: {
    background: "#201515",
    text: "#fff1eb",
    sidebar: "#2c1d1d",
    surface: "rgba(255, 255, 255, 0.05)",
    surfaceStrong: "rgba(56, 33, 27, 0.9)",
    surfaceSoft: "rgba(255, 255, 255, 0.03)",
    border: "rgba(251, 191, 36, 0.16)",
    mutedText: "rgba(255, 241, 235, 0.72)",
    accent: "#fb923c",
    accentStrong: "#f97316",
    success: "#4ade80",
    danger: "#f87171",
    shadow: "rgba(0, 0, 0, 0.34)"
  },
  plum: {
    background: "#1b1522",
    text: "#f6efff",
    sidebar: "#261f30",
    surface: "rgba(255, 255, 255, 0.05)",
    surfaceStrong: "rgba(43, 33, 57, 0.92)",
    surfaceSoft: "rgba(255, 255, 255, 0.03)",
    border: "rgba(216, 180, 254, 0.15)",
    mutedText: "rgba(246, 239, 255, 0.72)",
    accent: "#c084fc",
    accentStrong: "#a855f7",
    success: "#4ade80",
    danger: "#fb7185",
    shadow: "rgba(0, 0, 0, 0.34)"
  },
  paper: {
    background: "#f3ecd9",
    text: "#332718",
    sidebar: "#faf5e7",
    surface: "rgba(255, 252, 245, 0.78)",
    surfaceStrong: "rgba(255, 252, 245, 0.94)",
    surfaceSoft: "rgba(255, 248, 235, 0.62)",
    border: "rgba(51, 39, 24, 0.12)",
    mutedText: "rgba(51, 39, 24, 0.72)",
    accent: "#b7791f",
    accentStrong: "#975a16",
    success: "#2f855a",
    danger: "#c53030",
    shadow: "rgba(65, 47, 21, 0.12)"
  }
};

const boardThemes = {
  classic: {
    boardBg: "#1f2937",
    cellLight: "#374151",
    cellDark: "#111827",
    boardText: "#f9fafb",
    selectedOutline: "rgba(255, 255, 255, 0.72)",
    validMoveGlow: "rgba(56, 189, 248, 0.68)"
  },
  wood: {
    boardBg: "#4a2f24",
    cellLight: "#b08968",
    cellDark: "#7f5539",
    boardText: "#fff7ed",
    selectedOutline: "rgba(255, 247, 237, 0.78)",
    validMoveGlow: "rgba(245, 158, 11, 0.58)"
  },
  royal: {
    boardBg: "#1d2b53",
    cellLight: "#325ea8",
    cellDark: "#192f63",
    boardText: "#eff6ff",
    selectedOutline: "rgba(191, 219, 254, 0.84)",
    validMoveGlow: "rgba(96, 165, 250, 0.64)"
  },
  moss: {
    boardBg: "#1c2b22",
    cellLight: "#5f7a61",
    cellDark: "#2f4a36",
    boardText: "#f0fdf4",
    selectedOutline: "rgba(220, 252, 231, 0.82)",
    validMoveGlow: "rgba(74, 222, 128, 0.58)"
  },
  sunset: {
    boardBg: "#4a1f2d",
    cellLight: "#d97757",
    cellDark: "#7c2d3a",
    boardText: "#fff7ed",
    selectedOutline: "rgba(254, 215, 170, 0.84)",
    validMoveGlow: "rgba(251, 146, 60, 0.62)"
  },
  marble: {
    boardBg: "#cbd5e1",
    cellLight: "#f8fafc",
    cellDark: "#94a3b8",
    boardText: "#0f172a",
    selectedOutline: "rgba(15, 23, 42, 0.7)",
    validMoveGlow: "rgba(37, 99, 235, 0.46)"
  }
};

const pieceStyles = {
  gloss: {
    red: "radial-gradient(circle at 30% 30%, #ffb4b4, #ef4444 45%, #991b1b 100%)",
    green: "radial-gradient(circle at 30% 30%, #b8ffd2, #22c55e 45%, #166534 100%)",
    bobail: "radial-gradient(circle at 30% 30%, #fff2a6, #facc15 45%, #a16207 100%)",
    pieceBorder: "rgba(255, 255, 255, 0.26)",
    pieceShadow: "0 8px 18px rgba(0, 0, 0, 0.28)"
  },
  flat: {
    red: "#dc2626",
    green: "#16a34a",
    bobail: "#ca8a04",
    pieceBorder: "rgba(255, 255, 255, 0.14)",
    pieceShadow: "0 6px 14px rgba(0, 0, 0, 0.22)"
  },
  neon: {
    red: "#ff0055",
    green: "#00ff88",
    bobail: "#ffee00",
    pieceBorder: "rgba(255, 255, 255, 0.3)",
    pieceShadow: "0 0 16px rgba(255, 255, 255, 0.18)"
  },
  pastel: {
    red: "linear-gradient(145deg, #fda4af, #fb7185)",
    green: "linear-gradient(145deg, #86efac, #4ade80)",
    bobail: "linear-gradient(145deg, #fde68a, #fbbf24)",
    pieceBorder: "rgba(255, 255, 255, 0.36)",
    pieceShadow: "0 8px 18px rgba(0, 0, 0, 0.18)"
  },
  obsidian: {
    red: "linear-gradient(145deg, #7f1d1d, #ef4444)",
    green: "linear-gradient(145deg, #14532d, #22c55e)",
    bobail: "linear-gradient(145deg, #713f12, #f59e0b)",
    pieceBorder: "rgba(255, 255, 255, 0.2)",
    pieceShadow: "0 10px 22px rgba(0, 0, 0, 0.34)"
  },
  candy: {
    red: "linear-gradient(145deg, #fb7185, #e11d48)",
    green: "linear-gradient(145deg, #2dd4bf, #0f766e)",
    bobail: "linear-gradient(145deg, #f9a8d4, #db2777)",
    pieceBorder: "rgba(255, 255, 255, 0.3)",
    pieceShadow: "0 8px 18px rgba(31, 41, 55, 0.24)"
  }
};

export function ThemeProvider({ children }) {
  const [appTheme, setAppTheme] = useState(localStorage.getItem("appTheme") || "dark");
  const [boardTheme, setBoardTheme] = useState(localStorage.getItem("boardTheme") || "classic");
  const [pieceTheme, setPieceTheme] = useState(localStorage.getItem("pieceTheme") || "gloss");

  useEffect(() => {
    const applyVars = (obj) => {
      Object.entries(obj).forEach(([key, value]) => {
        document.documentElement.style.setProperty(`--${key}`, value);
      });
    };

    applyVars(appThemes[appTheme] || appThemes.dark);
    applyVars(boardThemes[boardTheme] || boardThemes.classic);
    applyVars(pieceStyles[pieceTheme] || pieceStyles.gloss);

    localStorage.setItem("appTheme", appTheme);
    localStorage.setItem("boardTheme", boardTheme);
    localStorage.setItem("pieceTheme", pieceTheme);
  }, [appTheme, boardTheme, pieceTheme]);

  return (
    <ThemeContext.Provider
      value={{
        appTheme,
        setAppTheme,
        boardTheme,
        setBoardTheme,
        pieceTheme,
        setPieceTheme
      }}
    >
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  return useContext(ThemeContext);
}
