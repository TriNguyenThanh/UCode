import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  typography: {
    fontFamily: "'Be Vietnam Pro', 'Inter', ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'",
  },
  palette: {
    primary: {
      main: '#FACB01', // Vàng - màu chính
      contrastText: '#00275e', // Text màu xanh đậm trên nền vàng
    },
    secondary: {
      main: '#00275e', // Xanh đậm - màu phụ
      contrastText: '#FACB01', // Text màu vàng trên nền xanh
    },
    background: {
      default: '#f5f5f5',
      paper: '#ffffff',
    },
  },
})
