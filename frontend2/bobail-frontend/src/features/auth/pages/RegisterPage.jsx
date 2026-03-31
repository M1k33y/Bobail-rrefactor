import { useState } from "react";
import { register } from "../api/authApi";
import { useNavigate } from "react-router-dom";
import "../styles/RegisterPage.css";

export default function RegisterPage() {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [nickname, setNickname] = useState("");
  const [errors, setErrors] = useState({});

  const validate = () => {
    const newErrors = {};

    if (!nickname) {
      newErrors.nickname = "Nickname is required";
    } else if (nickname.length < 3) {
      newErrors.nickname = "Minimum 3 characters";
    }

    if (!email) {
      newErrors.email = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(email)) {
      newErrors.email = "Invalid email format";
    }

    if (!password) {
      newErrors.password = "Password is required";
    } else if (password.length < 6) {
      newErrors.password = "Minimum 6 characters";
    }

    if (!confirmPassword) {
      newErrors.confirmPassword = "Please confirm password";
    } else if (password !== confirmPassword) {
      newErrors.confirmPassword = "Passwords do not match";
    }

    return newErrors;
  };

  const handleRegister = async () => {
    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      await register(email, password,nickname);
      navigate("/login");
    } catch {
      setErrors({ general: "Registration failed" });
    }
  };

  return (
    <div className="register-container">
      <div className="register-card">
        <h2>Create Account</h2>
        <p className="register-subtitle">
          Join and start playing
        </p>

        <input
          className={`register-input ${errors.nickname ? "error" : ""}`}
          placeholder="Nickname"
          value={nickname}
          onChange={(e) => {
            setNickname(e.target.value);
            setErrors((prev) => ({ ...prev, nickname: null }));
          }}
        />
        {errors.nickname && (
          <span className="error-text">{errors.nickname}</span>
        )}


        <input
          className={`register-input ${errors.email ? "error" : ""}`}
          placeholder="Email"
          value={email}
          onChange={(e) => {
            setEmail(e.target.value);
            setErrors((prev) => ({ ...prev, email: null }));
          }}
        />
        {errors.email && <span className="error-text">{errors.email}</span>}

        <input
          className={`register-input ${errors.password ? "error" : ""}`}
          placeholder="Password"
          type="password"
          value={password}
          onChange={(e) => {
            setPassword(e.target.value);
            setErrors((prev) => ({ ...prev, password: null }));
          }}
        />
        {errors.password && (
          <span className="error-text">{errors.password}</span>
        )}

        <input
          className={`register-input ${errors.confirmPassword ? "error" : ""}`}
          placeholder="Confirm Password"
          type="password"
          value={confirmPassword}
          onChange={(e) => {
            setConfirmPassword(e.target.value);
            setErrors((prev) => ({ ...prev, confirmPassword: null }));
          }}
        />
        {errors.confirmPassword && (
          <span className="error-text">{errors.confirmPassword}</span>
        )}

        {errors.general && (
          <span className="error-text">{errors.general}</span>
        )}

        <button className="register-button" onClick={handleRegister}>
          Create Account
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