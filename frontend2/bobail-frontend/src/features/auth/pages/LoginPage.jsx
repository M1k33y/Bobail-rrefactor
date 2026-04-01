import { useState, useEffect } from "react";
import { useAuth } from "../hooks/useAuth";
import { useNavigate, useLocation } from "react-router-dom";
import "../styles/LoginPage.css";
import { Eye, EyeOff, Mail, Lock } from "lucide-react";

export default function LoginPage() {
  const { loginUser } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const successMessage = location.state?.success;

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);


  useEffect(() => {
    if (location.state?.success) {
      window.history.replaceState({}, document.title);
    }
  }, []);

  const validate = () => {
    const newErrors = {};

    const trimmedEmail = email.trim();
    const trimmedPassword = password.trim();

    if (!trimmedEmail) {
      newErrors.email = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(trimmedEmail)) {
      newErrors.email = "Invalid email format";
    }

    if (!trimmedPassword) {
      newErrors.password = "Password is required";
    } else if (trimmedPassword.length < 6) {
      newErrors.password = "Minimum 6 characters";
    }

    return newErrors;
  };

  const handleLogin = async () => {
    if (loading) return;

    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      setLoading(true);
      setErrors({});

      await loginUser(email.trim(), password.trim());

      navigate(location.state?.from || "/");
    } catch {
      setErrors({ general: "Invalid email or password" });
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleLogin();
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Welcome Back</h2>
        <p className="login-subtitle">Log in to continue playing</p>

        {/*SUCCESS MESSAGE */}
        {successMessage && (
          <span className="success-text">{successMessage}</span>
        )}

        {/* EMAIL */}
        <div className="input-wrapper">
          <Mail className="input-icon left" size={18} />

          <input
            className={`login-input ${errors.email ? "error" : ""}`}
            placeholder="Email"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              setErrors((prev) => ({ ...prev, email: null, general: null }));
            }}
            onKeyDown={handleKeyDown}
            autoComplete="email"
          />
        </div>
        {errors.email && <span className="error-text">{errors.email}</span>}

        {/* PASSWORD */}
        <div className="input-wrapper">
          <Lock className="input-icon left" size={18} />

          <input
            className={`login-input ${errors.password ? "error" : ""}`}
            placeholder="Password"
            type={showPassword ? "text" : "password"}
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              setErrors((prev) => ({ ...prev, password: null, general: null }));
            }}
            onKeyDown={handleKeyDown}
            autoComplete="current-password"
          />

          <button
            type="button"
            className="eye-button"
            onClick={() => setShowPassword((prev) => !prev)}
          >
            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        </div>
        {errors.password && (
          <span className="error-text">{errors.password}</span>
        )}

        {/* GENERAL ERROR */}
        {errors.general && (
          <span className="error-text">{errors.general}</span>
        )}

        {/* BUTTON */}
        <button
          className="login-button"
          onClick={handleLogin}
          disabled={loading}
        >
          {loading ? "Logging in..." : "Login"}
        </button>

        <p className="login-footer">
          Don't have an account?{" "}
          <span onClick={() => navigate("/register")}>
            Register
          </span>
        </p>
      </div>
    </div>
  );
}