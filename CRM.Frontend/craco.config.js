// craco.config.js - Custom React App Configuration
// Enables webpack filesystem caching for faster incremental builds

const path = require('path');

module.exports = {
  webpack: {
    configure: (webpackConfig, { env, paths }) => {
      // Enable persistent filesystem cache (huge speedup for rebuilds)
      webpackConfig.cache = {
        type: 'filesystem',
        version: '1.0',
        cacheDirectory: path.resolve(__dirname, 'node_modules/.cache/webpack'),
        store: 'pack',
        buildDependencies: {
          config: [__filename],
          tsconfig: [path.resolve(__dirname, 'tsconfig.json')],
          package: [path.resolve(__dirname, 'package.json')],
        },
        // Cache invalidation based on these dependencies
        name: `${env}-${process.env.npm_package_version}`,
      };

      // Optimize for production builds
      if (env === 'production') {
        // Use parallel terser for faster minification
        if (webpackConfig.optimization && webpackConfig.optimization.minimizer) {
          webpackConfig.optimization.minimizer.forEach((plugin) => {
            if (plugin.constructor.name === 'TerserPlugin') {
              plugin.options.parallel = true;
            }
          });
        }

        // Code splitting for better caching
        webpackConfig.optimization.splitChunks = {
          chunks: 'all',
          maxInitialRequests: 25,
          minSize: 20000,
          cacheGroups: {
            // Vendor chunks - rarely change
            vendor: {
              test: /[\\/]node_modules[\\/]/,
              name(module) {
                // Group by package name for better caching
                const packageName = module.context.match(
                  /[\\/]node_modules[\\/](.*?)([\\/]|$)/
                )?.[1];
                return `vendor.${packageName?.replace('@', '')}`;
              },
              priority: 10,
            },
            // MUI components
            mui: {
              test: /[\\/]node_modules[\\/]@mui[\\/]/,
              name: 'vendor.mui',
              priority: 20,
              chunks: 'all',
            },
            // React core
            react: {
              test: /[\\/]node_modules[\\/](react|react-dom|react-router)[\\/]/,
              name: 'vendor.react',
              priority: 20,
              chunks: 'all',
            },
            // Common app code
            common: {
              minChunks: 2,
              priority: 5,
              reuseExistingChunk: true,
            },
          },
        };
      }

      return webpackConfig;
    },
  },
  
  // Babel configuration for faster builds
  babel: {
    plugins: [
      // Remove console.log in production
      process.env.NODE_ENV === 'production' && ['transform-remove-console', { exclude: ['error', 'warn'] }],
    ].filter(Boolean),
  },
};
