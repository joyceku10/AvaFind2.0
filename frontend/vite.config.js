import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Build output goes straight into the backend's static dir so one App
// Service serves both API and UI.
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../backend/static',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': 'http://127.0.0.1:8000',
    },
  },
})
