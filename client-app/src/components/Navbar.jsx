import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

export default function Navbar() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  if (!user) return null;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="navbar">
      <div>
        <Link to="/">Projects</Link>
        <Link to="/tasks">My Tasks</Link>
        <Link to="/profile">Profile</Link>
        {isAdmin && <Link to="/admin/users">Users (Admin)</Link>}
      </div>
      <div className="flex gap-8" style={{ alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          {user.profilePictureUrl && (
            <img 
              src={user.profilePictureUrl} 
              alt="Profile" 
              style={{ width: '32px', height: '32px', borderRadius: '50%', objectFit: 'cover' }}
            />
          )}
          <span className="muted" style={{ color: '#e5e7eb' }}>
            {user.fullName} ({isAdmin ? 'Administrator' : 'Employee'})
          </span>
        </div>
        <button className="btn btn-secondary" onClick={handleLogout}>Logout</button>
      </div>
    </div>
  );
}
