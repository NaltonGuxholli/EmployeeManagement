import api from './client';

// ---------- Auth ----------
export const login = (email, password) => api.post('/auth/login', { email, password });

// ---------- Users ----------
export const getUsers = () => api.get('/users');
export const getUser = (id) => api.get(`/users/${id}`);
export const createUser = (payload) => api.post('/users', payload);
export const updateUser = (id, payload) => api.put(`/users/${id}`, payload);
export const deleteUser = (id) => api.delete(`/users/${id}`);
export const getMe = () => api.get('/users/me');
export const updateMe = (payload) => api.put('/users/me', payload);
export const changeMyPassword = (payload) => api.post('/users/me/change-password', payload);
export const uploadMyProfilePicture = (file) => {
  const formData = new FormData();
  formData.append('file', file);
  return api.post('/users/me/profile-picture', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};

// ---------- Projects ----------
export const getProjects = () => api.get('/projects');
export const getProject = (id) => api.get(`/projects/${id}`);
export const createProject = (payload) => api.post('/projects', payload);
export const updateProject = (id, payload) => api.put(`/projects/${id}`, payload);
export const deleteProject = (id) => api.delete(`/projects/${id}`);
export const addProjectMember = (id, employeeId) => api.post(`/projects/${id}/members`, { employeeId });
export const removeProjectMember = (id, employeeId) => api.delete(`/projects/${id}/members/${employeeId}`);

// ---------- Tasks ----------
export const getTasks = (projectId) => api.get('/tasks', { params: projectId ? { projectId } : {} });
export const getMyTasks = () => api.get('/tasks/my');
export const getTask = (id) => api.get(`/tasks/${id}`);
export const createTask = (payload) => api.post('/tasks', payload);
export const updateTask = (id, payload) => api.put(`/tasks/${id}`, payload);
export const assignTask = (id, employeeId) => api.post(`/tasks/${id}/assign`, { employeeId });
export const completeTask = (id) => api.post(`/tasks/${id}/complete`);
export const toggleTaskCompletion = (id) => api.post(`/tasks/${id}/toggle-completion`);
export const deleteTask = (id) => api.delete(`/tasks/${id}`);
