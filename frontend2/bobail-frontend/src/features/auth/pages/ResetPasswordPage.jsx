import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Eye, EyeOff, KeyRound, Lock } from "lucide-react";
import { resetPassword } from "../api/authApi";
import "../styles/LoginPage.css";

export default function ResetPasswordPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [token, setToken] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    setToken(params.get("token") || "");
  }, [location.search]);
  const passwordRegex = /^(?=.*[A-Z])(?=.*\d).{8,}$/;
  const validate = () => {
    const nextErrors = {};

    if (!token.trim()) {
      nextErrors.token = "Reset token is required";
    }

    if (!password.trim()) {
      nextErrors.password = "Password is required";
    } else if (!passwordRegex.test(password.trim())) {
      nextErrors.password =
        "Min 8 chars, 1 uppercase letter and 1 number required";
    }

    if (!confirmPassword.trim()) {
      nextErrors.confirmPassword = "Please confirm password";
    } else if (password.trim() !== confirmPassword.trim()) {
      nextErrors.confirmPassword = "Passwords do not match";
    }

    return nextErrors;
  };

  const handleSubmit = async () => {
    if (loading) return;

    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      setLoading(true);
      setErrors({});
      await resetPassword(token.trim(), password.trim());
      setSuccess(true);
    } catch (error) {
      setErrors({ general: error.message || "Could not reset password" });
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleSubmit();
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Choose a New Password</h2>
        

        {success ? (
          <>
            <span className="success-text">
              Password updated successfully. You can sign in now.
            </span>
            <button
              className="login-button"
              onClick={() =>
                navigate("/login", {
                  state: { success: "Password reset successfully. Please log in." },
                })
              }
            >
              Go to Login
            </button>
          </>
        ) : (
          <>
            {/* <div className="input-wrapper">
              <KeyRound className="input-icon left" size={18} />
              <input
                className={`login-input ${errors.token ? "error" : ""}`}
                placeholder="Reset token"
                value={token}
                onChange={(e) => {
                  setToken(e.target.value);
                  setErrors((prev) => ({ ...prev, token: null, general: null }));
                }}
                onKeyDown={handleKeyDown}
              />
            </div>
            {errors.token && <span className="error-text">{errors.token}</span>} */}

            <div className="input-wrapper">
              <Lock className="input-icon left" size={18} />
              <input
                className={`login-input ${errors.password ? "error" : ""}`}
                placeholder="New password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  setErrors((prev) => ({ ...prev, password: null, general: null }));
                }}
                onKeyDown={handleKeyDown}
                autoComplete="new-password"
              />
              <button
                type="button"
                className="eye-button"
                onClick={() => setShowPassword((prev) => !prev)}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.password && <span className="error-text">{errors.password}</span>}

            <div className="input-wrapper">
              <Lock className="input-icon left" size={18} />
              <input
                className={`login-input ${errors.confirmPassword ? "error" : ""}`}
                placeholder="Confirm new password"
                type={showConfirmPassword ? "text" : "password"}
                value={confirmPassword}
                onChange={(e) => {
                  setConfirmPassword(e.target.value);
                  setErrors((prev) => ({
                    ...prev,
                    confirmPassword: null,
                    general: null,
                  }));
                }}
                onKeyDown={handleKeyDown}
                autoComplete="new-password"
              />
              <button
                type="button"
                className="eye-button"
                onClick={() => setShowConfirmPassword((prev) => !prev)}
              >
                {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.confirmPassword && (
              <span className="error-text">{errors.confirmPassword}</span>
            )}

            {errors.general && <span className="error-text">{errors.general}</span>}

            <button
              className="login-button"
              onClick={handleSubmit}
              disabled={loading}
            >
              {loading ? "Updating..." : "Reset Password"}
            </button>
          </>
        )}

        <p className="login-footer">
          Back to{" "}
          <span onClick={() => navigate("/login")}>
            Login
          </span>
        </p>
      </div>
    </div>
  );
}
