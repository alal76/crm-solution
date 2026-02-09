// craco.config.js - Custom React App Configuration
// Enables webpack filesystem caching for faster incremental builds

const path = require('path');
const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer');

// Bundle size budgets (in bytes)
const BUNDLE_SIZE_LIMITS = {
  // Main entry point should stay under 500KB
  mainEntryLimit: 500 * 1024,
  // Individual vendor chunks should stay under 250KB
  vendorChunkLimit: 250 * 1024,
  // Total bundle should stay under 2MB gzipped
  totalBundleLimit: 2 * 1024 * 1024,
};

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

        // Add bundle size budgets using webpack performance hints
        webpackConfig.performance = {
          hints: 'warning',
          maxEntrypointSize: BUNDLE_SIZE_LIMITS.mainEntryLimit,
          maxAssetSize: BUNDLE_SIZE_LIMITS.vendorChunkLimit,
          assetFilter: (assetFilename) => {
            // Only check JS and CSS files
            return /\.(js|css)$/i.test(assetFilename);
          },
        };
      }

      // Add bundle analyzer in analyze mode
      if (process.env.ANALYZE === 'true') {
        webpackConfig.plugins.push(
          new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'bundle-report.html',
            openAnalyzer: true,
            generateStatsFile: true,
            statsFilename: 'bundle-stats.json',
          })
        );
      }

      // Add bundle analyzer in CI mode (generates JSON only)
      if (process.env.ANALYZE_JSON === 'true') {
        webpackConfig.plugins.push(
          new BundleAnalyzerPlugin({
            analyzerMode: 'disabled',
            generateStatsFile: true,
            statsFilename: 'bundle-stats.json',
            statsOptions: { source: false },
          })
        );
      }

      return webpackConfig;
    },
  },
  
  // Fix webpack-dev-server v5 compatibility with CRA 5
  devServer: (devServerConfig) => {
    // Remove deprecated options from CRA 5 that webpack-dev-server v5 doesn't support
    delete devServerConfig.onAfterSetupMiddleware;
    delete devServerConfig.onBeforeSetupMiddleware;
    return devServerConfig;
  },

  // Babel configuration for faster builds
  babel: {
    plugins: [
      // Remove console.log in production
      process.env.NODE_ENV === 'production' && ['transform-remove-console', { exclude: ['error', 'warn'] }],
    ].filter(Boolean),
  },
};
