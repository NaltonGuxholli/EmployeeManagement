import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import * as api from '../api/endpoints';

const statusMap = {
    0: 'Open',
    1: 'In Progress',
    2: 'Completed'
};

const statusBadgeClass = (status) => {
    if (status === 2 || status === 'Completed') return 'badge badge-completed';
    if (status === 1 || status === 'InProgress') return 'badge badge-inprogress';
    return 'badge badge-open';
};

const getStatusName = (status) => {
    if (typeof status === 'number') return statusMap[status];
    return status;
};

export default function MyTasksPage() {
    const [tasks, setTasks] = useState([]);
    const [error, setError] = useState(null);
    const [expandedTaskId, setExpandedTaskId] = useState(null);

    const load = async () => {
        try {
            const { data } = await api.getMyTasks();
            setTasks(data);
        } catch (err) {
            setError(err.response?.data?.message || 'Could not load tasks.');
        }
    };

    useEffect(() => { load(); }, []);

    const handleToggleCompletion = async (id) => {
        try {
            await api.toggleTaskCompletion(id);
            load();
        } catch (err) {
            alert(err.response?.data?.message || 'Could not toggle task completion.');
        }
    };

    const expandedTask = tasks.find((t) => t.id === expandedTaskId) || null;

    return (
        <div className="container">
            <h2>My Tasks</h2>
            {error && <div className="error-text">{error}</div>}
            <table className="card">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Project</th>
                        <th>Status</th>
                        <th>Due</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {tasks.map((t) => {
                        const isSelected = expandedTaskId === t.id;
                        return (
                            <tr key={t.id} style={isSelected ? { background: 'rgba(0,0,0,0.03)' } : undefined}>
                                <td
                                    onClick={() => setExpandedTaskId(isSelected ? null : t.id)}
                                    style={{ cursor: t.description ? 'pointer' : 'default' }}
                                    title={t.description ? 'Click to view description' : undefined}
                                >
                                    <strong>{t.title}</strong>
                                    {t.description && (
                                        <span className="muted" style={{ marginLeft: 6, fontSize: 12 }}>
                                            {isSelected ? '▲' : '▼'}
                                        </span>
                                    )}
                                </td>
                                <td><Link to={`/projects/${t.projectId}`}>{t.projectName}</Link></td>
                                <td>
                                    <input
                                        type="checkbox"
                                        checked={t.status === 2}
                                        onChange={() => handleToggleCompletion(t.id)}
                                        style={{ cursor: 'pointer' }}
                                        title={getStatusName(t.status)}
                                    />
                                    <span className={statusBadgeClass(t.status)} style={{ marginLeft: '8px' }}>
                                        {getStatusName(t.status)}
                                    </span>
                                </td>
                                <td>{t.dueDate ? new Date(t.dueDate).toLocaleDateString() : '-'}</td>
                                <td>
                                    <button
                                        className="btn btn-secondary"
                                        onClick={() => handleToggleCompletion(t.id)}
                                        title={t.status === 2 ? 'Mark as Open' : 'Mark as Done'}
                                    >
                                        {t.status === 2 ? 'Reopen' : 'Done'}
                                    </button>
                                </td>
                            </tr>
                        );
                    })}
                    {tasks.length === 0 && (
                        <tr><td colSpan="5" className="muted">No tasks assigned to you.</td></tr>
                    )}
                </tbody>
            </table>

            {expandedTask && expandedTask.description && (
                <div className="card" style={{ marginTop: 12 }}>
                    <div className="flex-between" style={{ marginBottom: 8 }}>
                        <h4 style={{ margin: 0 }}>{expandedTask.title}</h4>
                        <button className="btn btn-secondary" style={{ padding: '2px 8px', fontSize: 12 }} onClick={() => setExpandedTaskId(null)}>
                            Close
                        </button>
                    </div>
                    <div
                        className="muted"
                        style={{ maxHeight: 220, overflowY: 'auto', whiteSpace: 'pre-wrap', lineHeight: 1.5 }}
                    >
                        {expandedTask.description}
                    </div>
                </div>
            )}
        </div>
    );
}