import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../features/auth/hooks/useAuth";

export default function ProtectedRoute({ children, requiredRole }) {
  const { isAuthenticated, role } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        state={{ from: location.pathname }}
        replace
      />
    );
  }

  if (requiredRole && role !== requiredRole) {
    return <Navigate to="/" replace />;
  }

  return children;
}
