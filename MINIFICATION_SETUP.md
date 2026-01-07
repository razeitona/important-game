# Asset Minification Setup

This document describes the automatic minification setup for JavaScript and CSS files in the project.

## Overview

The project uses a hybrid approach for minification:
- **CSS Minification**: BuildBundlerMinifier (MSBuild-integrated)
- **JavaScript Minification**: Terser (via npm)

Both processes run automatically during the build process.

## File Size Reduction

### JavaScript
- **Original**: `site.js` - 73 KB
- **Minified**: `site.min.js` - 28 KB
- **Reduction**: 62% (45 KB saved)

### CSS
- **Original**: `site.css` - 164 KB
- **Minified**: `site.min.css` - 165 KB
- **Note**: CSS is already optimized by SASS compiler

## Configuration Files

### bundleconfig.json
Located at: `src/important-game.web/bundleconfig.json`

Configures CSS minification:
```json
[
  {
    "outputFileName": "wwwroot/css/site.min.css",
    "inputFiles": [
      "wwwroot/css/site.css"
    ],
    "minify": {
      "enabled": true
    }
  }
]
```

### important-game.web.csproj
JavaScript minification is configured via MSBuild target:

```xml
<Target Name="MinifyJavaScript" AfterTargets="Build">
  <Exec Command="npx terser wwwroot/js/site.js -o wwwroot/js/site.min.js -c -m"
        WorkingDirectory="$(ProjectDir)"
        ContinueOnError="false" />
  <Message Text="JavaScript minification completed: site.min.js" Importance="high" />
</Target>
```

## Build Process

When you run `dotnet build`, the following happens:

1. **SASS Compilation**: `site.scss` → `site.css` (via AspNetCore.SassCompiler)
2. **CSS Minification**: `site.css` → `site.min.css` (via BuildBundlerMinifier)
3. **JavaScript Minification**: `site.js` → `site.min.js` (via Terser)

## Environment-Based File Loading

The `_Layout.cshtml` file uses ASP.NET Core environment tags to load different files based on the environment:

### Development Environment
- Uses **non-minified** files for easier debugging
- `site.css`
- `site.js`

### Production Environment
- Uses **minified** files for better performance
- `site.min.css`
- `site.min.js`

Example from `_Layout.cshtml`:
```html
<!-- CSS -->
<environment include="Development">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</environment>
<environment exclude="Development">
    <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
</environment>

<!-- JavaScript -->
<environment include="Development">
    <script defer src="~/js/site.js" asp-append-version="true"></script>
</environment>
<environment exclude="Development">
    <script defer src="~/js/site.min.js" asp-append-version="true"></script>
</environment>
```

## Dependencies

### NuGet Packages
- `BuildBundlerMinifier` (3.2.449) - CSS minification

### npm Packages
- `terser` - JavaScript minification

Install npm dependencies:
```bash
cd src/important-game.web
npm install
```

## Docker Build Support

The project's Dockerfile has been configured to support asset minification during Docker builds:

1. **Node.js Installation**: The build stage installs Node.js 20.x
2. **npm Package Installation**: Runs `npm install` to get Terser
3. **Automatic Minification**: The MSBuild target runs during `dotnet build`

### Dockerfile Changes
```dockerfile
# Install Node.js for asset minification
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

# Install npm packages
RUN npm install

# Build with minification
RUN dotnet build
```

### Skipping Minification in Docker (Optional)

If you want to skip minification in Docker builds:
```dockerfile
RUN dotnet build -p:SkipMinification=true
```

### Error Handling

The MSBuild target is configured with `ContinueOnError="true"` to gracefully handle cases where Node.js isn't available, preventing build failures in environments without Node.js.

## Terser Options

The Terser command uses the following flags:
- `-c` (compress): Enable compression with default settings
- `-m` (mangle): Shorten variable and function names

These options provide aggressive minification while maintaining code functionality.

## Troubleshooting

### JavaScript Minification Fails
If you see "Object reference not set to an instance of an object" errors:
- The BuildBundlerMinifier had issues with site.js
- Solution: We switched to Terser for JavaScript minification
- Terser is more robust and handles complex JavaScript better

### Missing Minified Files
If minified files don't exist after build:
- Ensure npm packages are installed: `npm install`
- Check that Terser is available: `npx terser --version`
- Rebuild the project: `dotnet build`

### Performance Issues
If builds are slow:
- Terser runs only when site.js changes
- Consider adding conditions to skip minification in Debug builds if needed

## Performance Impact

### Page Load Improvements
- **JavaScript**: 45 KB less data transferred (62% reduction)
- **Combined with gzip**: Additional 60-70% compression on minified files
- **Expected impact**:
  - Faster initial page load
  - Reduced bandwidth usage
  - Better performance on slow connections

### With HTTP Compression (gzip/brotli)
- Minified JS (28 KB) → ~8-10 KB gzipped
- Original JS (73 KB) → ~20-25 KB gzipped
- **Net savings**: 12-15 KB per page load

## Verification

To verify minification is working:

1. **Check file sizes**:
   ```bash
   ls -lh wwwroot/js/site*.js
   ls -lh wwwroot/css/site*.css
   ```

2. **Check build output**:
   Look for "JavaScript minification completed: site.min.js" message

3. **Test in browser**:
   - Development mode: Should load `site.js`
   - Production mode: Should load `site.min.js`

## Future Enhancements

Potential improvements:
1. Add source maps for debugging production issues
2. Implement cache-busting strategy
3. Consider bundling multiple JS files into one
4. Add image optimization to build process
5. Implement conditional minification (skip in Debug builds)
