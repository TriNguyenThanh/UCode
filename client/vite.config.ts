import { reactRouter } from '@react-router/dev/vite'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vite'
import tsconfigPaths from 'vite-tsconfig-paths'
import devtoolsJson from 'vite-plugin-devtools-json'

export default defineConfig({
  css: {
    devSourcemap: true
  },
  server: {
    host: '0.0.0.0',
    port: 3000,
    allowedHosts: [
      'tigerishly-farinose-shaunda.ngrok-free.dev',
      'localhost',
      '.ngrok-free.dev',
      '.ngrok.io',
    ]
  },
  preview: {
    host: '0.0.0.0',
    port: 3000
  },
  plugins: [tailwindcss(), reactRouter(), tsconfigPaths(), devtoolsJson()],
  
  optimizeDeps: {
    force: true, 
    entries: ['app/**/*.{ts,tsx}'], 
    include: [
      '@mui/material',
      '@mui/icons-material', 
      'react-syntax-highlighter',
    ],
  },
  
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('@mui/material') || id.includes('@mui/icons-material')) {
            return 'mui' 
          }
          
          if (id.includes('react-syntax-highlighter')) {
            return 'syntax-highlighter'
          }
          
          if (id.includes('node_modules')) {
            return 'vendor'
          }
        },
      
      },
    },
    chunkSizeWarningLimit: 1000,
  },
})