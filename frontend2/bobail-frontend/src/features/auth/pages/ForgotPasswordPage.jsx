import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Mail } from "lucide-react";
import { forgotPassword } from "../api/authApi";
import "../styles/LoginPage.css";

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);

  const validate = () => {
    const trimmedEmail = email.trim();

    if (!trimmedEmail) {
      return { email: "Email is required" };
    }

    if (!/\S+@\S+\.\S+/.test(trimmedEmail)) {
      return { email: "Invalid email format" };
    }

    return {};
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
      const response = await forgotPassword(email.trim());
      setResult(response);
    } catch (error) {
      setErrors({ general: error.message || "Could not start reset flow" });
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
        <h2>Reset Password</h2>
        <p className="login-subtitle">
          Enter your email and we&apos;ll prepare a reset link.
        </p>

        {!result && (
          <>
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
          </>
        )}

        {errors.general && <span className="error-text">{errors.general}</span>}

        {result && <span className="success-text">{result.message}</span>}

        {!result && (
          <button
            className="login-button"
            onClick={handleSubmit}
            disabled={loading}
          >
            {loading ? "Preparing..." : "Send Reset Link"}
          </button>
        )}

        <p className="login-footer">
          Remembered it?{" "}
          <span onClick={() => navigate("/login")}>
            Back to login
          </span>
        </p>
      </div>
    </div>
  );
}
