import { render } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';

export function renderWithRouter(element, { route='/', routes=[{ path: route, element }] } = {}) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <Routes>
        {routes.map(r => <Route key={r.path} path={r.path} element={r.element} />)}
      </Routes>
    </MemoryRouter>
  );
}
