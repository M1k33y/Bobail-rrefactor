import { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import "../styles/LoginPage.css";
import { useLocation } from "react-router-dom";

export default function LoginPage() {
  const { loginUser } = useAuth();
  const navigate = useNavigate();
    const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [errors, setErrors] = useState({});

  const validate = () => {
    const newErrors = {};

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

    return newErrors;
  };

  const handleLogin = async () => {
    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      await loginUser(email, password);
      navigate(location.state?.from || "/");
    } catch {
      setErrors({ general: "Invalid email or password" });
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Welcome Back</h2>
        <p className="login-subtitle">Log in to continue playing</p>

        <input
          className={`login-input ${errors.email ? "error" : ""}`}
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        {errors.email && <span className="error-text">{errors.email}</span>}

        <input
          className={`login-input ${errors.password ? "error" : ""}`}
          placeholder="Password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        {errors.password && (
          <span className="error-text">{errors.password}</span>
        )}

        {errors.general && (
          <span className="error-text">{errors.general}</span>
        )}

        <button className="login-button" onClick={handleLogin}>
          Login
        </button>

        <p className="login-footer">
          Don’t have an account?{" "}
          <span onClick={() => navigate("/register")}>
            Register
          </span>
        </p>
      </div>
    </div>
  );
}