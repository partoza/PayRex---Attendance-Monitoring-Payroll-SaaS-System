/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
    './**/*.cshtml',
    './**/*.html',
    './**/*.razor',
        './**/*.js',
        "./node_modules/flowbite/**/*.js"

  ],
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: '#007BFF' },
        'page-bg': '#F8FBFF',
        'ph-blue': '#0038A8',
        'ph-red': '#CE1126',
        'ph-yellow': '#FCD116',
        'soft-blue': '#E0F2FE'
      },
      fontFamily: {
        inter: ['Inter', 'sans-serif'],
      },
      backgroundImage: {
        geometric: "radial-gradient(circle at 10% 20%, rgba(0, 123, 255, 0.05) 0%, transparent 20%), radial-gradient(circle at 90% 80%, rgba(139, 92, 246, 0.05) 0%, transparent 20%)",
      },
      boxShadow: {
        glass: '0 8px 32px 0 rgba(31, 38, 135, 0.07)',
        float: '0 20px 60px rgba(0, 123, 255, 0.1)'
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' }
        }
      },
      animation: {
        float: 'float 6s ease-in-out infinite'
      }
    }
  },
    plugins: [
        require('flowbite/plugin')
    ]
}
