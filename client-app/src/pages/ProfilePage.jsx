import React, { useEffect, useState } from 'react';
import * as api from '../api/endpoints';

export default function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [form, setForm] = useState({ firstName: '', lastName: '', position: '', department: '', phoneNumber: '' });
  const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '' });
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  const load = async () => {
    const { data } = await api.getMe();
    setProfile(data);
    setForm({
      firstName: data.firstName,
      lastName: data.lastName,
      position: data.position || '',
      department: data.department || '',
      phoneNumber: data.phoneNumber || ''
    });
  };

  useEffect(() => { load(); }, []);

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setError(null);
    setMessage(null);
    try {
      await api.updateMe(form);
      setMessage('Profile updated.');
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Could not update profile.');
    }
  };

  const handleUploadPicture = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    try {
      await api.uploadMyProfilePicture(file);
      setMessage('Profile picture updated.');
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Could not upload picture.');
    }
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    setError(null);
    setMessage(null);
    try {
      await api.changeMyPassword(pwForm);
      setMessage('Password changed.');
      setPwForm({ currentPassword: '', newPassword: '' });
    } catch (err) {
      setError(err.response?.data?.message || 'Could not change password.');
    }
  };

  if (!profile) return <div className="container muted">Loading...</div>;

  return (
    <div className="container">
      <h2>My Profile</h2>
      {message && <div className="muted" style={{ color: '#065f46' }}>{message}</div>}
      {error && <div className="error-text">{error}</div>}

      <div className="card flex gap-8" style={{ alignItems: 'center' }}>
        <img
          className="profile-picture"
          src={profile.profilePictureUrl || 'https://placehold.co/96x96?text=No+Photo'}
          alt="Profile"
        />
        <div>
          <label>Upload new picture</label>
          <input type="file" accept="image/*" onChange={handleUploadPicture} />
        </div>
      </div>

      <form className="card" onSubmit={handleSaveProfile}>
        <h3>Profile details</h3>
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
          <label>Phone number</label>
          <input value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
        </div>
        <button className="btn btn-primary" type="submit">Save changes</button>
      </form>

      <form className="card" onSubmit={handleChangePassword}>
        <h3>Change password</h3>
        <div className="form-row">
          <label>Current password</label>
          <input type="password" value={pwForm.currentPassword}
            onChange={(e) => setPwForm({ ...pwForm, currentPassword: e.target.value })} required />
        </div>
        <div className="form-row">
          <label>New password</label>
          <input type="password" value={pwForm.newPassword}
            onChange={(e) => setPwForm({ ...pwForm, newPassword: e.target.value })} required minLength={6} />
        </div>
        <button className="btn btn-primary" type="submit">Change password</button>
      </form>
    </div>
  );
}
