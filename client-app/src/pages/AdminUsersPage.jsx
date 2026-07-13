import React, { useEffect, useState } from 'react';
import * as api from '../api/endpoints';

const emptyForm = {
  email: '', password: '', firstName: '', lastName: '', position: '', department: '', phoneNumber: '', role: 'Employee'
};

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState(null);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(null);

  const load = async () => {
    try {
      const { data } = await api.getUsers();
      setUsers(data);
    } catch (err) {
      setError(err.response?.data?.message || 'Could not load users.');
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.createUser(form);
      setForm(emptyForm);
      setShowForm(false);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Could not create user.');
    }
  };

  const startEdit = (u) => {
    setEditingId(u.id);
    setEditForm({
      firstName: u.firstName,
      lastName: u.lastName,
      position: u.position || '',
      department: u.department || '',
      phoneNumber: u.phoneNumber || '',
      isActive: u.isActive,
      role: u.roles[0] || 'Employee'
    });
  };

  const handleSaveEdit = async (id) => {
    try {
      await api.updateUser(id, editForm);
      setEditingId(null);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Could not update user.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Remove this user? This is only possible if they have no open tasks assigned.')) return;
    try {
      await api.deleteUser(id);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Could not remove user.');
    }
  };

  return (
    <div className="container">
      <div className="flex-between">
        <h2>Users</h2>
        <button className="btn btn-primary" onClick={() => setShowForm((s) => !s)}>
          {showForm ? 'Cancel' : '+ New User'}
        </button>
      </div>

      {showForm && (
        <form className="card" onSubmit={handleCreate}>
          <div className="form-row">
            <label>Email</label>
            <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
          </div>
          <div className="form-row">
            <label>Temporary password</label>
            <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required minLength={6} />
          </div>
          <div className="form-row">
            <label>First name</label>
            <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
          </div>
          <div className="form-row">
            <label>Last name</label>
            <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
          </div>
          <div className="form-row">
            <label>Position</label>
            <input value={form.position} onChange={(e) => setForm({ ...form, position: e.target.value })} />
          </div>
          <div className="form-row">
            <label>Department</label>
            <input value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} />
          </div>
          <div className="form-row">
            <label>Role</label>
            <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
              <option value="Employee">Employee</option>
              <option value="Administrator">Administrator</option>
            </select>
          </div>
          <button className="btn btn-primary" type="submit">Create user</button>
        </form>
      )}

      {error && <div className="error-text">{error}</div>}

      <table className="card">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Role</th>
            <th>Active</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {users.map((u) => (
            <tr key={u.id}>
              {editingId === u.id ? (
                <>
                  <td>
                    <input value={editForm.firstName} onChange={(e) => setEditForm({ ...editForm, firstName: e.target.value })} />
                    <input value={editForm.lastName} onChange={(e) => setEditForm({ ...editForm, lastName: e.target.value })} style={{ marginTop: 4 }} />
                  </td>
                  <td>{u.email}</td>
                  <td>
                    <select value={editForm.role} onChange={(e) => setEditForm({ ...editForm, role: e.target.value })}>
                      <option value="Employee">Employee</option>
                      <option value="Administrator">Administrator</option>
                    </select>
                  </td>
                  <td>
                    <input type="checkbox" checked={editForm.isActive} onChange={(e) => setEditForm({ ...editForm, isActive: e.target.checked })} />
                  </td>
                  <td>
                    <div className="flex gap-8">
                      <button className="btn btn-primary" onClick={() => handleSaveEdit(u.id)}>Save</button>
                      <button className="btn btn-secondary" onClick={() => setEditingId(null)}>Cancel</button>
                    </div>
                  </td>
                </>
              ) : (
                <>
                  <td>{u.firstName} {u.lastName}</td>
                  <td>{u.email}</td>
                  <td>{u.roles.join(', ')}</td>
                  <td>{u.isActive ? 'Yes' : 'No'}</td>
                  <td>
                    <div className="flex gap-8">
                      <button className="btn btn-secondary" onClick={() => startEdit(u)}>Edit</button>
                      <button className="btn btn-danger" onClick={() => handleDelete(u.id)}>Remove</button>
                    </div>
                  </td>
                </>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
