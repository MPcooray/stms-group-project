import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import ProtectedRoute from '../ProtectedRoute.jsx';

// Need to mock the context using the relative path from __tests__ to src/context
vi.mock('../../context/AuthContext.jsx', () => ({ useAuth: () => ({ isAuthenticated: false }) }));

describe('ProtectedRoute', () => {
  it('redirects when not authenticated', () => {
    render(
      <MemoryRouter initialEntries={['/secret']}>
        <Routes>
          <Route path='/login' element={<div>Login Page</div>} />
          <Route path='/secret' element={<ProtectedRoute><div>Secret</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    );
    expect(screen.getByText(/Login Page/)).toBeInTheDocument();
  });
});
