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
  plugins: [tailwindcss(), reactRouter(), tsconfigPaths(), devtoolsJson()],
  
  // ✅ Pre-optimize tất cả MUI icons và dependencies
  optimizeDeps: {
    include: [
      // MUI Core
      '@mui/material',
      '@mui/material/Box',
      '@mui/material/Typography',
      '@mui/material/Paper',
      '@mui/material/Table',
      '@mui/material/TableBody',
      '@mui/material/TableCell',
      '@mui/material/TableContainer',
      '@mui/material/TableHead',
      '@mui/material/TableRow',
      '@mui/material/Chip',
      '@mui/material/IconButton',
      '@mui/material/Dialog',
      '@mui/material/DialogTitle',
      '@mui/material/DialogContent',
      '@mui/material/DialogActions',
      '@mui/material/Button',
      '@mui/material/Divider',
      '@mui/material/CircularProgress',
      '@mui/material/Alert',
      '@mui/material/Accordion',
      '@mui/material/AccordionSummary',
      '@mui/material/AccordionDetails',
      '@mui/material/Grid',
      '@mui/material/Card',
      '@mui/material/CardContent',
      '@mui/material/Tabs',
      '@mui/material/Tab',
      '@mui/material/Select',
      '@mui/material/MenuItem',
      '@mui/material/FormControl',
      '@mui/material/TextField',
      
      // MUI Icons (tất cả icons đang dùng)
      '@mui/icons-material',
      '@mui/icons-material/CheckCircle',
      '@mui/icons-material/Cancel',
      '@mui/icons-material/Visibility',
      '@mui/icons-material/VisibilityOff',
      '@mui/icons-material/ExpandMore',
      '@mui/icons-material/AccessTime',
      '@mui/icons-material/Memory',
      '@mui/icons-material/Code',
      '@mui/icons-material/CheckCircleOutline',
      '@mui/icons-material/ErrorOutline',
      '@mui/icons-material/WarningAmber',
      '@mui/icons-material/Build',
      '@mui/icons-material/SkipNext',
      '@mui/icons-material/Error',
      '@mui/icons-material/Warning',
      '@mui/icons-material/Close',
      '@mui/icons-material/ArrowBack',
      '@mui/icons-material/PlayArrow',
      '@mui/icons-material/Send',
      '@mui/icons-material/RestartAlt',
      '@mui/icons-material/Assignment',
      '@mui/icons-material/Class',
      '@mui/icons-material/ArrowForward',
      '@mui/icons-material/Pending',
      '@mui/icons-material/Info',
      '@mui/icons-material/Menu',
      '@mui/icons-material/Home',
      '@mui/icons-material/Person',
      '@mui/icons-material/Settings',
      '@mui/icons-material/Logout',
      '@mui/icons-material/Add',
      '@mui/icons-material/Edit',
      '@mui/icons-material/Delete',
      '@mui/icons-material/Search',
      
      // Syntax Highlighter
      'react-syntax-highlighter',
      'react-syntax-highlighter/dist/esm/styles/prism',
    ],
  },
  
  // ✅ Build optimization
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          // Tách MUI ra chunks riêng
          'mui-core': ['@mui/material'],
          'mui-icons': [
            '@mui/icons-material/CheckCircle',
            '@mui/icons-material/Cancel',
            '@mui/icons-material/Visibility',
            '@mui/icons-material/VisibilityOff',
            '@mui/icons-material/ExpandMore',
            '@mui/icons-material/AccessTime',
            '@mui/icons-material/Memory',
            '@mui/icons-material/Code',
            '@mui/icons-material/CheckCircleOutline',
            '@mui/icons-material/ErrorOutline',
            '@mui/icons-material/WarningAmber',
            '@mui/icons-material/Build',
            '@mui/icons-material/SkipNext',
            '@mui/icons-material/Error',
            '@mui/icons-material/Warning',
            '@mui/icons-material/Close',
            '@mui/icons-material/ArrowBack',
            '@mui/icons-material/PlayArrow',
            '@mui/icons-material/Send',
            '@mui/icons-material/RestartAlt',
            '@mui/icons-material/Assignment',
            '@mui/icons-material/Class',
            '@mui/icons-material/ArrowForward',
            '@mui/icons-material/Pending',
            '@mui/icons-material/Info',
            '@mui/icons-material/Menu',
            '@mui/icons-material/Home',
            '@mui/icons-material/Person',
            '@mui/icons-material/Settings',
            '@mui/icons-material/Logout',
            '@mui/icons-material/Add',
            '@mui/icons-material/Edit',
            '@mui/icons-material/Delete',
            '@mui/icons-material/Search',
          ],
          // Tách syntax highlighter (khá nặng ~200KB)
          'syntax-highlighter': [
            'react-syntax-highlighter',
            'react-syntax-highlighter/dist/esm/styles/prism',
          ],
        },
      },
    },
    // Tăng warning limit cho MUI
    chunkSizeWarningLimit: 1000,
  },
})
