#!/bin/bash

# Script tạo .app bundle cho macOS

APP_NAME="PhoenixLogistic"
BUNDLE_NAME="${APP_NAME}.app"
EXECUTABLE_NAME="PhoenixLogisticPrintLabel.Avalonia"

# Xác định kiến trúc (arm64 hoặc x64)
if [[ $(uname -m) == "arm64" ]]; then
    RUNTIME="osx-arm64"
else
    RUNTIME="osx-x64"
fi

echo "🔨 Building for ${RUNTIME}..."

# Build & publish
dotnet publish -c Release -r ${RUNTIME} --self-contained true -p:PublishSingleFile=true

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "📦 Creating app bundle..."

# Tạo cấu trúc thư mục
rm -rf "${BUNDLE_NAME}"
mkdir -p "${BUNDLE_NAME}/Contents/MacOS"
mkdir -p "${BUNDLE_NAME}/Contents/Resources"

# Copy executable
cp "bin/Release/net9.0/${RUNTIME}/publish/${EXECUTABLE_NAME}" "${BUNDLE_NAME}/Contents/MacOS/"

# Tạo Info.plist
cat > "${BUNDLE_NAME}/Contents/Info.plist" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>PhoenixLogisticPrintLabel.Avalonia</string>
    <key>CFBundleIdentifier</key>
    <string>com.phoenixlogistics.printlabel</string>
    <key>CFBundleName</key>
    <string>PhoenixLogistic Print Label</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
</dict>
</plist>
EOF

# Copy icon nếu có
if [ -f "Assets/logo-small.icns" ]; then
    cp "Assets/logo-small.icns" "${BUNDLE_NAME}/Contents/Resources/"
fi

# Set permissions
chmod +x "${BUNDLE_NAME}/Contents/MacOS/${EXECUTABLE_NAME}"

echo "✅ App bundle created: ${BUNDLE_NAME}"
echo ""
echo "Để cài đặt:"
echo "  cp -r ${BUNDLE_NAME} /Applications/"
echo ""
echo "Để chạy:"
echo "  open ${BUNDLE_NAME}"
