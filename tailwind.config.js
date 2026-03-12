/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml"
  ],
  // Tie Tailwind dark mode to the same attribute the app already uses.
  darkMode: ['attribute', 'data-theme'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0d6efd',
          dark: '#0b5ed7',
        }
      }
    }
  },
  plugins: [],
}

