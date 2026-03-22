import { createContext, useContext, useEffect, useState } from "react";

const ThemeContext = createContext();

const appThemes = {
  dark: {
  background: "#111827",     
  text: "#e5e7eb",            
  sidebar: "#1f2937"          
},
light: {
  background: "#d3e2ee",      
  text: "#111827",            
  sidebar: "#e5e7eb"          
},
ocean: {
  background: "#0b1220",
  text: "#e0f2fe",
  sidebar: "#0f1c2e"
}
};

const boardThemes = {
  classic: {
  boardBg: "#1f2937",
  cellLight: "#374151",
  cellDark: "#111827",
  boardText: "#f9fafb"
},
wood: {
  boardBg: "#3e2723",
  cellLight: "#a1887f",
  cellDark: "#5d4037",
  boardText: "#ffffff"
},
blue: {
  boardBg: "#1e3a8a",
  cellLight: "#430213",
  cellDark: "#112054",
  boardText: "#ffffff"
}
};

const pieceStyles = {
  default: {
    red: "linear-gradient(145deg, #ff4d4d, #c70000)",
    green: "linear-gradient(145deg, #3ddc84, #1e7c4a)",
    bobail: "linear-gradient(145deg, #f5d142, #c9a000)"
  },
  flat: {
    red: "#ff4d4d",
    green: "#3ddc84",
    bobail: "#f5d142"
  },
  neon: {
    red: "#ff0055",
    green: "#00ff88",
    bobail: "#ffee00"
  }
};

export function ThemeProvider({ children }) {
  const [appTheme, setAppTheme] = useState(
    localStorage.getItem("appTheme") || "dark"
  );

  const [boardTheme, setBoardTheme] = useState(
    localStorage.getItem("boardTheme") || "classic"
  );

  const [pieceTheme, setPieceTheme] = useState(
    localStorage.getItem("pieceTheme") || "default"
  );

  useEffect(() => {
    const applyVars = (obj) => {
      Object.entries(obj).forEach(([key, value]) => {
        document.documentElement.style.setProperty(
          `--${key}`,
          value
        );
      });
    };

    applyVars(appThemes[appTheme]);
    applyVars(boardThemes[boardTheme]);
    applyVars(pieceStyles[pieceTheme]);

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