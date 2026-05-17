import { NavLink } from "react-router-dom";
import { LogIn, LogOut } from "lucide-react";

function SidebarUserProfile({ isAuthenticated, logout, nickname, role }) {
  if (!isAuthenticated) {
    return (
      <section className="sidebar-user-profile" aria-label="Account access">
        <NavLink to="/login" className="sidebar-login-card">
          <span className="sidebar-login-icon" aria-hidden="true">
            <LogIn size={18} />
          </span>

          <span className="sidebar-login-details">
            <span className="sidebar-login-title">Login</span>
            <span className="sidebar-login-subtitle">Guest </span>
          </span>
        </NavLink>
      </section>
    );
  }

  const username = nickname?.trim() || "Player";
  const avatarLetter = username.charAt(0).toUpperCase();

  return (
    <section className="sidebar-user-profile" aria-label="User profile">
      <div className="sidebar-user-card">
        <div className="sidebar-user-avatar" aria-hidden="true">
          {avatarLetter}
        </div>

        <div className="sidebar-user-details">
          <span className="sidebar-user-name" title={username}>
            {username}
          </span>
          <span className="sidebar-user-role">{role || "Player"}</span>
        </div>

        <button
          type="button"
          className="sidebar-logout-button"
          onClick={logout}
          aria-label="Log out"
          title="Log out"
        >
          <LogOut size={17} />
        </button>
      </div>
    </section>
  );
}

export default SidebarUserProfile;
