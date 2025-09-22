import { Link } from "react-router-dom";
import { useState } from "react";
import { useAuthStore } from "../store/authStore";

export default function Navbar() {
  const { token, setToken } = useAuthStore();
  const [isCollapsed, setIsCollapsed] = useState(true);

  const handleLogout = () => {
    setToken(null);
  };

  const toggleNavbar = () => {
    setIsCollapsed(!isCollapsed);
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-light bg-white border-bottom">
      <div className="container-fluid">
        <Link to="/" className="navbar-brand text-decoration-none">
          React
        </Link>
        
        <button 
          className="navbar-toggler" 
          type="button" 
          onClick={toggleNavbar}
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        
        <div className={`collapse navbar-collapse ${!isCollapsed ? 'show' : ''}`} id="navbarNav">
          <ul className="navbar-nav me-auto">
            <li className="nav-item">
              <Link to="/" className="nav-link">
                Home
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/users" className="nav-link">
                Users
              </Link>
            </li>
          </ul>
          
          <ul className="navbar-nav">
            <li className="nav-item">
              {token ? (
                <button 
                  onClick={handleLogout} 
                  className="btn btn-outline-danger btn-sm"
                >
                  Logout
                </button>
              ) : (
                <Link to="/login" className="btn btn-primary btn-sm">
                  Login
                </Link>
              )}
            </li>
          </ul>
        </div>
      </div>
    </nav>
  );
}
