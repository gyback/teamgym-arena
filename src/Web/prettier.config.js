// @ts-check

/** @type {import('prettier').Config} */
export default {
  printWidth: 120,
  singleQuote: true,
  plugins: ['prettier-plugin-tailwindcss'],
  tailwindStylesheet: './src/app/styles.css',
  tailwindFunctions: ['cva', 'cn'],
};
