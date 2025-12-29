# PhoenixLogistic Print Label - Avalonia (macOS/Linux/Windows)

Ứng dụng in nhãn vận chuyển cho Phoenix Logistics, phiên bản cross-platform sử dụng Avalonia UI.

## Yêu cầu hệ thống

### macOS
- macOS 10.15 (Catalina) trở lên
- .NET 9.0 SDK
- Máy in đã được cấu hình trong System Preferences

### Linux
- .NET 9.0 SDK
- CUPS printing system
- X11 hoặc Wayland

### Windows
- Windows 10/11
- .NET 9.0 SDK

## Cài đặt .NET 9.0 SDK

### macOS
```bash
# Sử dụng Homebrew
brew install dotnet-sdk

# Hoặc tải từ:
# https://dotnet.microsoft.com/download/dotnet/9.0
```

### Linux (Ubuntu/Debian)
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
```

### Windows
Tải và cài đặt từ: https://dotnet.microsoft.com/download/dotnet/9.0

## Build và chạy

### 1. Clone hoặc copy project
```bash
cd PhoenixLogisticPrintLabel.Avalonia
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Build project
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### 4. Chạy ứng dụng
```bash
# Debug
dotnet run

# Release
dotnet run -c Release
```

## Tạo ứng dụng độc lập (Self-contained)

### macOS (Apple Silicon - M1/M2/M3)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

### macOS (Intel)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

### Linux (x64)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### Windows (x64)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Ứng dụng sẽ được tạo trong thư mục: `bin/Release/net9.0/{runtime}/publish/`

## Tạo .app bundle cho macOS

Sau khi publish, tạo app bundle:

```bash
# 1. Tạo cấu trúc thư mục
mkdir -p PhoenixLogistic.app/Contents/MacOS
mkdir -p PhoenixLogistic.app/Contents/Resources

# 2. Copy executable
cp bin/Release/net9.0/osx-arm64/publish/PhoenixLogisticPrintLabel.Avalonia PhoenixLogistic.app/Contents/MacOS/

# 3. Tạo Info.plist
cat > PhoenixLogistic.app/Contents/Info.plist << 'EOF'
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
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# 4. Copy icon (nếu có)
# cp Assets/logo-small.icns PhoenixLogistic.app/Contents/Resources/

# 5. Set permissions
chmod +x PhoenixLogistic.app/Contents/MacOS/PhoenixLogisticPrintLabel.Avalonia

# 6. Chạy app
open PhoenixLogistic.app
```

## Cấu hình máy in trên macOS

1. Mở **System Settings** > **Printers & Scanners**
2. Thêm máy in của bạn
3. Đảm bảo máy in đang online và sẵn sàng
4. Kiểm tra danh sách máy in bằng Terminal:
   ```bash
   lpstat -p -d
   ```

## In thử nghiệm

Để test in một file PDF:
```bash
# In với máy in mặc định
lpr test.pdf

# In với máy in cụ thể
lpr -P "Ten_May_In" test.pdf
```

## Cấu trúc project

```
PhoenixLogisticPrintLabel.Avalonia/
├── App.axaml                  # Application resources và styles
├── App.axaml.cs              # Application code-behind
├── MainWindow.axaml          # Main window UI
├── MainWindow.axaml.cs       # Main window logic
├── Program.cs                # Entry point
├── PhoenixLogisticPrintLabel.Avalonia.csproj
├── Assets/                   # Images, icons
└── README.md
```

## Tính năng

- ✅ Đăng nhập API Phoenix Logistics
- ✅ Quét mã vạch đơn hàng
- ✅ Tải PDF label từ server
- ✅ In tự động với máy in đã chọn
- ✅ Hỗ trợ macOS (CUPS printing)
- ✅ Hỗ trợ Linux (CUPS printing)
- ✅ Cross-platform UI với Avalonia

## Khác biệt với phiên bản WPF

1. **UI Framework**: Avalonia thay vì WPF
2. **Printing**: 
   - macOS/Linux: Sử dụng `lpr` command (CUPS)
   - Windows: Cần implement riêng (hoặc dùng Spire.PDF với platform check)
3. **Password Box**: Avalonia dùng TextBox với PasswordChar thay vì PasswordBox
4. **MessageBox**: Avalonia không có MessageBox built-in, cần tạo custom dialog

## Troubleshooting

### Lỗi "command not found: dotnet"
```bash
# Thêm .NET vào PATH
export PATH=$PATH:$HOME/.dotnet
```

### Máy in không hiển thị
```bash
# Kiểm tra CUPS service
lpstat -r
# Nếu không chạy:
sudo cupsctl
```

### App không mở được trên macOS (Security warning)
```bash
# Cho phép app chạy
xattr -d com.apple.quarantine PhoenixLogistic.app
```

### Lỗi SSL/TLS khi kết nối API
```bash
# Update certificates
# macOS:
brew install ca-certificates
# Linux:
sudo apt-get install ca-certificates
```

## Phát triển

### Debug trong VS Code
```bash
# Install C# extension
code --install-extension ms-dotnettools.csharp

# Debug: F5
```

### Hot Reload
```bash
dotnet watch run
```

## License

Copyright © 2025 Phoenix Logistics

## Hỗ trợ

Email: support@phoenixlogistics.vn
