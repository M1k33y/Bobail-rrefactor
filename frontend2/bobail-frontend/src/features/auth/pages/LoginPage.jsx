import { useState, useEffect } from "react";
import { useAuth } from "../hooks/useAuth";
import { useNavigate, useLocation } from "react-router-dom";
import { resendVerification } from "../api/authApi";
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
  const [rememberMe, setRememberMe] = useState(true);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [resendLoading, setResendLoading] = useState(false);

  useEffect(() => {
    if (location.state?.success) {
      window.history.replaceState({}, document.title);
    }
  }, [location.state?.success]);

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

      await loginUser(email.trim(), password.trim(), rememberMe);

      navigate(location.state?.from || "/");
    } catch (error) {
      setErrors({ general: error.message || "Invalid email or password" });
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleLogin();
    }
  };

  const handleResendVerification = async () => {
    if (resendLoading) return;

    if (!email.trim()) {
      setErrors({ email: "Enter your email to resend verification" });
      return;
    }

    try {
      setResendLoading(true);
      const response = await resendVerification(email.trim());
      setErrors({ general: null });
      navigate("/login", {
        replace: true,
        state: { success: response.message },
      });
    } catch (error) {
      setErrors({ general: error.message || "Could not resend verification email" });
    } finally {
      setResendLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Welcome Back</h2>
        <p className="login-subtitle">Log in to continue playing</p>

        {successMessage && (
          <span className="success-text">{successMessage}</span>
        )}

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

        {errors.general && (
          <span className="error-text">{errors.general}</span>
        )}

        <div className="login-options">
          <label className="remember-me">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
            />
            <span>Remember me</span>
          </label>

          <button
            type="button"
            className="text-link-button"
            onClick={() => navigate("/forgot-password")}
          >
            Forgot password?
          </button>
        </div>

        <button
          className="login-button"
          onClick={handleLogin}
          disabled={loading}
        >
          {loading ? "Logging in..." : "Login"}
        </button>

        <button
          type="button"
          className="text-link-button"
          onClick={handleResendVerification}
          disabled={resendLoading}
        >
          {resendLoading ? "Sending verification..." : "Resend verification email"}
        </button>

        <p className="login-footer">
          Don&apos;t have an account?{" "}
          <span onClick={() => navigate("/register")}>
            Register
          </span>
        </p>
      </div>
    </div>
  );
}
