import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import * as api from "../api/endpoints";

const statusMap = { 0: "Open", 1: "In Progress", 2: "Completed" };

const statusBadgeClass = (status) => {
  if (status === 2 || status === "Completed") return "badge badge-completed";
  if (status === 1 || status === "InProgress") return "badge badge-inprogress";
  return "badge badge-open";
};

const getStatusName = (status) => (typeof status === "number" ? statusMap[status] : status);

export default function ProjectDetailPage() {
  const { id } = useParams();
  const projectId = Number(id);
  const { user, isAdmin } = useAuth();
  const [project, setProject] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [allEmployees, setAllEmployees] = useState([]);
  const [error, setError] = useState(null);
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [taskForm, setTaskForm] = useState({ title: "", description: "", assignedToId: "", dueDate: "" });
  const [newMemberId, setNewMemberId] = useState("");
  const [expandedTaskId, setExpandedTaskId] = useState(null);

  const load = async () => {
    try {
      const [{ data: projectData }, { data: tasksData }] = await Promise.all([
        api.getProject(projectId),
        api.getTasks(projectId)
      ]);
      setProject(projectData);
      setTasks(tasksData);
    } catch (err) {
      setError(err.response?.data?.message || "Could not load project.");
    }
  };

  useEffect(() => { load(); }, [projectId]);

  useEffect(() => {
    if (isAdmin) {
      api.getUsers().then(({ data }) => setAllEmployees(data)).catch(() => {});
    }
  }, [isAdmin]);

  const isMember = project?.members?.some((m) => m.userId === user.id);

  const handleCreateTask = async (e) => {
    e.preventDefault();
    try {
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const dueDate = taskForm.dueDate ? new Date(taskForm.dueDate) : null;
      if (dueDate && dueDate < today) {
        alert("Due date cannot be in the past.");
        return;
      }
      await api.createTask({
        title: taskForm.title,
        description: taskForm.description || null,
        projectId,
        assignedToId: taskForm.assignedToId ? Number(taskForm.assignedToId) : null,
        dueDate: taskForm.dueDate || null
      });
      setTaskForm({ title: "", description: "", assignedToId: "", dueDate: "" });
      setShowTaskForm(false);
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not create task.");
    }
  };

  const handleToggleCompletion = async (taskId) => {
    try {
      await api.toggleTaskCompletion(taskId);
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not toggle task completion.");
    }
  };

  const handleAssign = async (taskId, employeeId) => {
    if (!employeeId) return;
    try {
      await api.assignTask(taskId, Number(employeeId));
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not assign task.");
    }
  };

  const handleDeleteTask = async (taskId) => {
    if (!window.confirm("Remove this task?")) return;
    try {
      await api.deleteTask(taskId);
      if (expandedTaskId === taskId) setExpandedTaskId(null);
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not remove task.");
    }
  };

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!newMemberId) return;
    try {
      await api.addProjectMember(projectId, Number(newMemberId));
      setNewMemberId("");
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not add member.");
    }
  };

  const handleRemoveMember = async (employeeId) => {
    if (!window.confirm("Remove this employee from the project?")) return;
    try {
      await api.removeProjectMember(projectId, employeeId);
      load();
    } catch (err) {
      alert(err.response?.data?.message || "Could not remove member.");
    }
  };

  if (error) return <div className="container error-text">{error}</div>;
  if (!project) return <div className="container muted">Loading...</div>;

  const canCreateTask = isAdmin || isMember;
  const projectMemberOptions = project.members;
  const today = new Date();
  const todayString = today.toISOString().split("T")[0];
  const expandedTask = tasks.find((t) => t.id === expandedTaskId) || null;

  return (
    <div className="container">
      <h2>{project.name}</h2>
      {project.description && (
        <div
          className="card muted"
          style={{ maxHeight: 150, overflowY: "auto", whiteSpace: "pre-wrap", lineHeight: 1.5 }}
        >
          {project.description}
        </div>
      )}

      <div className="card">
        <h3>Members</h3>
        <ul>
          {project.members.map((m) => (
            <li key={m.userId}>
              {m.fullName} ({m.email})
              {isAdmin && (
                <button className="btn btn-danger" style={{ marginLeft: 10, padding: "2px 8px", fontSize: 12 }} onClick={() => handleRemoveMember(m.userId)}>
                  Remove
                </button>
              )}
            </li>
          ))}
        </ul>
        {isAdmin && (
          <form className="flex gap-8" onSubmit={handleAddMember}>
            <select value={newMemberId} onChange={(e) => setNewMemberId(e.target.value)}>
              <option value="">Select employee to add...</option>
              {allEmployees.filter((emp) => !project.members.some((m) => m.userId === emp.id)).map((emp) => (
                <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName} ({emp.email})</option>
              ))}
            </select>
            <button className="btn btn-secondary" type="submit">Add</button>
          </form>
        )}
      </div>

      <div className="flex-between">
        <h3>Tasks</h3>
        {canCreateTask && (
          <button className="btn btn-primary" onClick={() => setShowTaskForm((s) => !s)}>
            {showTaskForm ? "Cancel" : "+ New Task"}
          </button>
        )}
      </div>

      {showTaskForm && (
        <form className="card" onSubmit={handleCreateTask}>
          <div className="form-row">
            <label>Title</label>
            <input value={taskForm.title} onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })} required />
          </div>
          <div className="form-row">
            <label>Description</label>
            <textarea value={taskForm.description} onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })} />
          </div>
          <div className="form-row">
            <label>Assign to</label>
            <select value={taskForm.assignedToId} onChange={(e) => setTaskForm({ ...taskForm, assignedToId: e.target.value })}>
              <option value="">Unassigned</option>
              {projectMemberOptions.map((m) => (
                <option key={m.userId} value={m.userId}>{m.fullName}</option>
              ))}
            </select>
          </div>
          <div className="form-row">
            <label>Due date</label>
            <input type="date" value={taskForm.dueDate} onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })} min={todayString} />
          </div>
          <button className="btn btn-primary" type="submit">Create Task</button>
        </form>
      )}

      <table className="card">
        <thead>
          <tr>
            <th>Title</th>
            <th>Assigned To</th>
            <th>Status</th>
            <th>Due</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {tasks.map((t) => {
            const canModify = isAdmin || t.assignedToId === user.id;
            const isSelected = expandedTaskId === t.id;
            return (
              <tr key={t.id} style={isSelected ? { background: "rgba(0,0,0,0.03)" } : undefined}>
                <td
                  onClick={() => setExpandedTaskId(isSelected ? null : t.id)}
                  style={{ cursor: t.description ? "pointer" : "default" }}
                  title={t.description ? "Click to view description" : undefined}
                >
                  <strong>{t.title}</strong>
                  {t.description && (
                    <span className="muted" style={{ marginLeft: 6, fontSize: 12 }}>
                      {isSelected ? "▲" : "▼"}
                    </span>
                  )}
                </td>
                <td>
                  {isAdmin || t.assignedToId == null || t.assignedToId === user.id || t.createdById === user.id ? (
                    <select value={t.assignedToId || ""} onChange={(e) => handleAssign(t.id, e.target.value)}>
                      <option value="">Unassigned</option>
                      {projectMemberOptions.map((m) => (
                        <option key={m.userId} value={m.userId}>{m.fullName}</option>
                      ))}
                    </select>
                  ) : (
                    t.assignedToName || <span className="muted">Unassigned</span>
                  )}
                </td>
                <td>
                  <input type="checkbox" checked={t.status === 2} onChange={() => handleToggleCompletion(t.id)} disabled={!canModify} style={{ cursor: canModify ? "pointer" : "not-allowed" }} title={getStatusName(t.status)} />
                  <span className={statusBadgeClass(t.status)} style={{ marginLeft: "8px" }}>{getStatusName(t.status)}</span>
                </td>
                <td>{t.dueDate ? new Date(t.dueDate).toLocaleDateString() : "-"}</td>
                <td>
                  <div className="flex gap-8">
                    {canModify && (
                      <button className="btn btn-secondary" onClick={() => handleToggleCompletion(t.id)} title={t.status === 2 ? "Mark as Open" : "Mark as Done"}>
                        {t.status === 2 ? "Reopen" : "Done"}
                      </button>
                    )}
                    {isAdmin && (
                      <button className="btn btn-danger" onClick={() => handleDeleteTask(t.id)}>Delete</button>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
          {tasks.length === 0 && (
            <tr><td colSpan="5" className="muted">No tasks yet.</td></tr>
          )}
        </tbody>
      </table>

      {expandedTask && expandedTask.description && (
        <div className="card" style={{ marginTop: 12 }}>
          <div className="flex-between" style={{ marginBottom: 8 }}>
            <h4 style={{ margin: 0 }}>{expandedTask.title}</h4>
            <button className="btn btn-secondary" style={{ padding: "2px 8px", fontSize: 12 }} onClick={() => setExpandedTaskId(null)}>
              Close
            </button>
          </div>
          <div
            className="muted"
            style={{ maxHeight: 220, overflowY: "auto", whiteSpace: "pre-wrap", lineHeight: 1.5 }}
          >
            {expandedTask.description}
          </div>
        </div>
      )}
    </div>
  );
}