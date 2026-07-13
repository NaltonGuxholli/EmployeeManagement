import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import * as api from '../api/endpoints';

export default function ProjectsPage() {
  const { isAdmin } = useAuth();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ name: '', description: '', dueDate: '' });

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await api.getProjects();
      setProjects(data);
    } catch (err) {
      setError(err.response?.data?.message || 'Could not load projects.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      await api.createProject({
        name: form.name,
        description: form.description || null,
        dueDate: form.dueDate || null
      });
      setForm({ name: '', description: '', dueDate: '' });
      setShowForm(false);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Could not create project.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Remove this project? This is only possible if it has no open tasks.')) return;
    try {
      await api.deleteProject(id);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Could not remove project.');
    }
  };

  return (
    <div className="container">
      <div className="flex-between">
        <h2>Projects</h2>
        {isAdmin && (
          <button className="btn btn-primary" onClick={() => setShowForm((s) => !s)}>
            {showForm ? 'Cancel' : '+ New Project'}
          </button>
        )}
      </div>

      {showForm && (
        <form className="card" onSubmit={handleCreate}>
          <div className="form-row">
            <label>Name</label>
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          </div>
          <div className="form-row">
            <label>Description</label>
            <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          </div>
          <div className="form-row">
            <label>Due date</label>
            <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
          </div>
          <button className="btn btn-primary" type="submit">Create</button>
        </form>
      )}

      {error && <div className="error-text">{error}</div>}
      {loading ? (
        <p className="muted">Loading...</p>
      ) : projects.length === 0 ? (
        <p className="muted">No projects to show.</p>
      ) : (
        projects.map((p) => (
          <div className="card" key={p.id}>
            <div className="flex-between">
              <div>
                <h3 style={{ margin: '0 0 4px' }}>
                  <Link to={`/projects/${p.id}`}>{p.name}</Link>
                </h3>
                <p className="muted" style={{ margin: 0 }}>{p.description}</p>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div className="muted">{p.openTaskCount} open / {p.totalTaskCount} total tasks</div>
                <div className="muted">{p.members.length} member(s)</div>
                {isAdmin && (
                  <button className="btn btn-danger" style={{ marginTop: 8 }} onClick={() => handleDelete(p.id)}>
                    Remove
                  </button>
                )}
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  );
}
