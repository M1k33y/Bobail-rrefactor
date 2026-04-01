import { useState } from "react";
import { register } from "../api/authApi";
import { useNavigate } from "react-router-dom";
import "../styles/RegisterPage.css";
import { Eye, EyeOff, Mail, Lock, User } from "lucide-react";
export default function RegisterPage() {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [nickname, setNickname] = useState("");
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const validate = () => {
    const newErrors = {};

    const trimmedEmail = email.trim();
    const trimmedPassword = password.trim();
    const trimmedNickname = nickname.trim();

    if (!trimmedNickname) {
      newErrors.nickname = "Nickname is required";
    } else if (trimmedNickname.length < 3) {
      newErrors.nickname = "Minimum 3 characters";
    } else if (!/^[a-zA-Z0-9_]+$/.test(trimmedNickname)) {
      newErrors.nickname = "Only letters, numbers and _ allowed";
    }

    if (!trimmedEmail) {
      newErrors.email = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(trimmedEmail)) {
      newErrors.email = "Invalid email format";
    }

    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

    if (!trimmedPassword) {
      newErrors.password = "Password is required";
    } else if (!passwordRegex.test(trimmedPassword)) {
      newErrors.password =
        "Min 8 chars, 1 uppercase, 1 lowercase, 1 number";
    }

    if (!confirmPassword) {
      newErrors.confirmPassword = "Please confirm password";
    } else if (trimmedPassword !== confirmPassword.trim()) {
      newErrors.confirmPassword = "Passwords do not match";
    }

    return newErrors;
  };

  const handleRegister = async () => {
    if (loading) return;

    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      setLoading(true);
      setErrors({});

      await register(email.trim(), password.trim(), nickname.trim());

      navigate("/login", {
        state: { success: "Account created successfully!" },
      });
    } catch {
      setErrors({ general: "Registration failed" });
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleRegister();
    }
  };

  return (
    <div className="register-container">
      <div className="register-card">
        <h2>Create Account</h2>
        <p className="register-subtitle">
          Join and start playing
        </p>

        <div className="input-wrapper">
          <User className="input-icon left" size={18} />

          <input
            className={`register-input ${errors.nickname ? "error" : ""}`}
            placeholder="Nickname"
            value={nickname}
            onChange={(e) => {
              setNickname(e.target.value);
              setErrors((prev) => ({ ...prev, nickname: null }));
            }}
            onKeyDown={handleKeyDown}
          />
        </div>
        {errors.nickname && (
          <span className="error-text">{errors.nickname}</span>
        )}

        <div className="input-wrapper">
          <Mail className="input-icon left" size={18} />

          <input
            className={`register-input ${errors.email ? "error" : ""}`}
            placeholder="Email"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              setErrors((prev) => ({ ...prev, email: null }));
            }}
            onKeyDown={handleKeyDown}
          />
        </div>
        {errors.email && <span className="error-text">{errors.email}</span>}

        <div className="input-wrapper">
          <Lock className="input-icon left" size={18} />

          <input
            className={`register-input ${errors.password ? "error" : ""}`}
            placeholder="Password"
            type={showPassword ? "text" : "password"}
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              setErrors((prev) => ({ ...prev, password: null }));
            }}
            onKeyDown={handleKeyDown}
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

        <div className="input-wrapper">
          <Lock className="input-icon left" size={18} />

          <input
            className={`register-input ${errors.confirmPassword ? "error" : ""}`}
            placeholder="Confirm Password"
            type={showConfirmPassword ? "text" : "password"}
            value={confirmPassword}
            onChange={(e) => {
              setConfirmPassword(e.target.value);
              setErrors((prev) => ({ ...prev, confirmPassword: null }));
            }}
            onKeyDown={handleKeyDown}
          />

          <button
            type="button"
            className="eye-button"
            onClick={() =>
              setShowConfirmPassword((prev) => !prev)
            }
          >
            {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        </div>
        {errors.confirmPassword && (
          <span className="error-text">{errors.confirmPassword}</span>
        )}

        {errors.general && (
          <span className="error-text">{errors.general}</span>
        )}

        <button
          className="register-button"
          onClick={handleRegister}
          disabled={loading}
        >
          {loading ? "Creating..." : "Create Account"}
        </button>

        <p className="register-footer">
          Already have an account?{" "}
          <span onClick={() => navigate("/login")}>
            Login
          </span>
        </p>
      </div>
    </div>
  );
}