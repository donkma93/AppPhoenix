# Hướng dẫn nhanh - Build trên macOS

## 1. Cài đặt .NET SDK

```bash
# Dùng Homebrew (khuyên dùng)
brew install dotnet-sdk

# Hoặc tải từ Microsoft
# https://dotnet.microsoft.com/download/dotnet/9.0
```

Kiểm tra cài đặt:
```bash
dotnet --version
# Phải là 9.0.x
```

## 2. Copy images từ project cũ

```bash
# Copy logo và images
cp ../PhoenixLogisticPrintLabel/image/logo-small.png Assets/
cp ../PhoenixLogisticPrintLabel/image/logo-small.ico Assets/
```

## 3. Build & Run

```bash
# Restore packages lần đầu
dotnet restore

# Build
dotnet build

# Chạy
dotnet run
```

## 4. Tạo app cho macOS

### Apple Silicon (M1/M2/M3):
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

### Intel Mac:
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

File thực thi sẽ ở:
```
bin/Release/net9.0/osx-arm64/publish/PhoenixLogisticPrintLabel.Avalonia
```

## 5. Tạo .app bundle (tuỳ chọn)

```bash
# Chạy script tạo app bundle
chmod +x create-app-bundle.sh
./create-app-bundle.sh
```

Sau đó có thể copy `PhoenixLogistic.app` đến `/Applications`

## Yêu cầu hệ thống

- **macOS 11 (Big Sur)** trở lên
- .NET 9.0 SDK

## Lưu ý

- Máy in phải được cấu hình trong **System Settings > Printers & Scanners**
- App sử dụng lệnh `lpr` để in, cần CUPS service đang chạy
- Lần đầu chạy có thể cần cấp quyền trong System Settings > Privacy & Security
