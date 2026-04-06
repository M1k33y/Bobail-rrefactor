import { NavLink } from "react-router-dom";
import { useState } from "react";
import { Play, BookOpen, History } from "lucide-react";
import { Settings } from "lucide-react";
import "../styles/Sidebar.css";
function Sidebar() {
    const [playOpen, setPlayOpen] = useState(false);

    return (
        <div className="sidebar">

            <NavLink to="/" className="logo">
                Bobail
            </NavLink>

            <div
                className="menu-group"
                onMouseEnter={() => setPlayOpen(true)}
                onMouseLeave={() => setPlayOpen(false)}
            >
                <NavLink to="/" className="menu-item">
                    <Play size={18} />
                    <span>Play</span>
                </NavLink>

                <div className={`submenu ${playOpen ? "open" : ""}`}>
                    <NavLink to="/play/local" className="submenu-item">
                        2 Player
                    </NavLink>

                    <NavLink to="/play/online" className="submenu-item">
                        Play Online
                    </NavLink>

                    <NavLink to="/play/bot" className="submenu-item">
                        Play vs Bot
                    </NavLink>
                </div>
            </div>

            <NavLink to="/rules" className="menu-item single">
                <BookOpen size={18} />
                <span>Learn</span>
            </NavLink>

            <NavLink to="/game-history" className="menu-item single">
                <History size={18} />
                <span>Game History</span>
            </NavLink>

            <NavLink to="/settings" className="menu-item single">
                <Settings size={18} />
                <span>Settings</span>
            </NavLink>
        </div>
    );
}

export default Sidebar;
