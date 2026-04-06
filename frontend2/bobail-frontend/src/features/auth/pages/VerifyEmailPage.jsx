import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { verifyEmail } from "../api/authApi";
import "../styles/LoginPage.css";

export default function VerifyEmailPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [status, setStatus] = useState("loading");
  const [message, setMessage] = useState("Verifying your email...");
  const lastProcessedTokenRef = useRef(null);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = (params.get("token") || "").trim();

    if (!token) {
      setStatus("error");
      setMessage("Verification token is missing.");
      return;
    }

    if (lastProcessedTokenRef.current === token) {
      return;
    }

    lastProcessedTokenRef.current = token;

    const runVerification = async () => {
      try {
        const response = await verifyEmail(token);
        setStatus("success");
        setMessage(response.message || "Email verified successfully.");
      } catch (error) {
        setStatus("error");
        setMessage(error.message || "Email verification failed.");
      }
    };

    runVerification();
  }, [location.search]);

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Email Verification</h2>
        <p className="login-subtitle">
          {status === "loading"
            ? "Please wait while we confirm your account."
            : "Your account status is shown below."}
        </p>

        <span className={status === "success" ? "success-text" : "error-text"}>
          {message}
        </span>

        <button
          className="login-button"
          onClick={() =>
            navigate("/login", {
              state: status === "success" ? { success: message } : undefined,
            })
          }
        >
          Go to Login
        </button>
      </div>
    </div>
  );
}
