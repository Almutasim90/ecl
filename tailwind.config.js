/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml"
  ],
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

