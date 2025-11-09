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
      '.ngrok-free.dev', // Allow all ngrok-free.dev subdomains
      '.ngrok.io', // Allow all ngrok.io subdomains (if using paid ngrok)
    ]
  },
  preview: {
    port: 3000
  },
  plugins: [tailwindcss(), reactRouter(), tsconfigPaths(), devtoolsJson()]
})
