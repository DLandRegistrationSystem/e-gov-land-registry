import { defineConfig } from 'vite';

export default defineConfig({
  root: 'src',
  build: {
    outDir: '../wwwroot/js/index.html',
    emptyOutDir: true,
  }
});

// import { defineConfig } from 'vite';

// export default defineConfig({
//   build: {
//     rollupOptions: {
//       input: 'index.html'
//     }
//   }
// });