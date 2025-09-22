import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../store/authStore";
import { authApi } from "../api/authApi";

interface User {
  id: string;
  userName: string;
  email: string;
  emailConfirmed: boolean;
}

export default function Users() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { token } = useAuthStore();

  // load users
  const fetchUsers = async () => {
    try {
      setLoading(true);
      const res = await authApi.getAll();
      setUsers(res.data);
    } catch (err) {
      console.error("Failed to fetch users", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Check authentication first
    if (!token) {
      navigate('/login');
      return;
    }
    
    fetchUsers();
  }, [token, navigate]);

  // Check authentication
  if (!token) {
    return null; // Will redirect in useEffect
  }

  return (
    <div className="container mt-4">
      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h2 mb-0">Users</h1>
      </div>

      {/* Users Table */}
      <div className="table-responsive">
        <table className="table table-striped">
                    <thead>
            <tr>
              <th>ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Email Confirmed</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.id}</td>
                <td>{user.userName}</td>
                <td>{user.email}</td>
                <td>
                  <span className={`badge ${user.emailConfirmed ? 'bg-success' : 'bg-warning'}`}>
                    {user.emailConfirmed ? 'Confirmed' : 'Pending'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {users.length === 0 && !loading && (
        <div className="text-center mt-4">
          <p className="text-muted">No users found.</p>
        </div>
      )}
    </div>
  );
}

