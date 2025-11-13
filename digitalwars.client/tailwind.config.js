/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'secondary': '#141c2f',
        'tertiary': '#1e293b',
        'accent': '#a78bfa',
        'lgray-blue': "#1E293B",
        'lgray-accent': "#2B374B",
          primary: {
            50: '#f5f3ff',
            100: '#ede9fe',
            200: '#ddd6fe',
            300: '#c4b5fd',
            400: '#a78bfa',
            500: '#8b5cf6',
            600: '#7c3aed',
            700: '#6d28d9',
            800: '#5b21b6',
            900: '#4c1d95',
            950: '#2e1065'
          },
          surface: {
            0: '#ffffff',
            50: '#f8fafc',
            100: '#f1f5f9',
            200: '#e2e8f0',
            300: '#cbd5e1',
            400: '#94a3b8',
            500: '#64748b',
            600: '#475569',
            700: '#334155',
            800: '#1e293b',
            850: '#172033',
            900: '#0f172a',
            950: '#020617'
          }
        },
      fontFamily: {
        'nasalization': ['Nasalization', 'sans-serif'],
        'mulish': ['Mulish', 'sans-serif'],
        'sans': ['Mulish', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [],
}