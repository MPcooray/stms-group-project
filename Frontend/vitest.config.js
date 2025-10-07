import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.js'],
    globals: true,
    exclude: [
      'ui-tests/**',        // ensure Playwright specs are not executed by Vitest
      'node_modules/**',
      'dist/**',
      'coverage/**'
    ],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov'],
      reportsDirectory: './coverage',
      exclude: [
        'node_modules/**',
        'src/tests/**',
        'src/**/*.test.{js,jsx,ts,tsx}',
        'src/**/__tests__/**',
        'ui-tests/**'
      ],
      lines: 70,
      functions: 70,
      branches: 60,
      statements: 70
    }
  }
});
