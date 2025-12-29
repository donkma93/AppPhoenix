# Hướng dẫn cài đặt trên macOS

## 📦 File đã build sẵn

Ứng dụng đã được build cho 2 loại Mac:

### 1. **Apple Silicon (M1/M2/M3/M4)** - Khuyên dùng cho Mac mới
📁 Thư mục: `bin/Release/net9.0/osx-arm64/publish/`
📄 File chính: `PhoenixLogisticPrintLabel.Avalonia`
💾 Dung lượng: ~91 MB

### 2. **Intel Mac** - Cho Mac cũ
📁 Thư mục: `bin/Release/net9.0/osx-x64/publish/`
📄 File chính: `PhoenixLogisticPrintLabel.Avalonia`
💾 Dung lượng: ~84 MB

## 🚀 Cách cài đặt

### Bước 1: Copy file sang Mac

**Cách 1: USB/External Drive**
```bash
# Copy toàn bộ thư mục publish vào USB
# Sau đó trên Mac, copy vào thư mục Applications
```

**Cách 2: AirDrop** (nhanh nhất)
- Chọn thư mục `publish` → Click phải → Share → AirDrop
- Chọn Mac của bạn

**Cách 3: Cloud (Google Drive/Dropbox)**
- Upload thư mục `publish` lên cloud
- Tải về trên Mac

### Bước 2: Cấp quyền thực thi

Mở Terminal trên Mac và chạy:

```bash
# Di chuyển đến thư mục chứa file (ví dụ)
cd ~/Downloads/publish

# Cấp quyền thực thi
chmod +x PhoenixLogisticPrintLabel.Avalonia
chmod +x *.dylib

# Xóa quarantine attribute (bắt buộc!)
xattr -d com.apple.quarantine *
```

### Bước 3: Chạy ứng dụng

```bash
# Chạy trực tiếp
./PhoenixLogisticPrintLabel.Avalonia

# Hoặc double-click vào file trong Finder
```

## 🎁 Tạo .app Bundle (Tuỳ chọn - Dễ sử dụng hơn)

### Cách 1: Dùng script tự động

1. Copy file `create-app-bundle.sh` vào thư mục `publish`
2. Chạy script:

```bash
cd ~/Downloads/publish
chmod +x create-app-bundle.sh
./create-app-bundle.sh
```

3. Kéo thả `PhoenixLogistic.app` vào `/Applications`

### Cách 2: Tạo thủ công

```bash
cd ~/Downloads/publish

# 1. Tạo cấu trúc bundle
mkdir -p PhoenixLogistic.app/Contents/MacOS
mkdir -p PhoenixLogistic.app/Contents/Resources

# 2. Copy files
cp PhoenixLogisticPrintLabel.Avalonia PhoenixLogistic.app/Contents/MacOS/
cp *.dylib PhoenixLogistic.app/Contents/MacOS/

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
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# 4. Set permissions
chmod +x PhoenixLogistic.app/Contents/MacOS/*
xattr -d com.apple.quarantine PhoenixLogistic.app

# 5. Copy vào Applications
cp -r PhoenixLogistic.app /Applications/

# 6. Chạy
open /Applications/PhoenixLogistic.app
```

## 🖨️ Cấu hình máy in trên macOS

### 1. Thêm máy in
- Mở **System Settings** (hoặc System Preferences)
- Chọn **Printers & Scanners**
- Click **+** để thêm máy in
- Chọn máy in của bạn và thêm

### 2. Kiểm tra máy in

```bash
# Xem danh sách máy in
lpstat -p -d

# Kết quả mẫu:
# printer HP_LaserJet is idle. enabled since ...
# printer Canon_MG3600 is idle. enabled since ...
```

### 3. Test in

```bash
# In file PDF test
lpr -P "Ten_May_In" file.pdf

# In với máy in mặc định
lpr file.pdf
```

## ⚙️ Yêu cầu hệ thống

- **macOS**: 10.15 (Catalina) trở lên
- **Bộ nhớ**: Tối thiểu 4GB RAM
- **Dung lượng**: 200MB trống
- **Máy in**: Đã được cấu hình trong System Settings

## 🔧 Xử lý lỗi thường gặp

### Lỗi: "App can't be opened because it is from an unidentified developer"

```bash
# Xóa quarantine attribute
xattr -d com.apple.quarantine PhoenixLogisticPrintLabel.Avalonia

# Hoặc cho phép trong System Settings
# System Settings → Privacy & Security → Security
# Click "Open Anyway"
```

### Lỗi: "Permission denied"

```bash
# Cấp quyền thực thi
chmod +x PhoenixLogisticPrintLabel.Avalonia
```

### Lỗi: Không thấy máy in

```bash
# Kiểm tra CUPS service
lpstat -r

# Restart CUPS nếu cần
sudo launchctl stop org.cups.cupsd
sudo launchctl start org.cups.cupsd
```

### Lỗi: Thiếu thư viện .NET

```bash
# Ứng dụng này là self-contained nên KHÔNG cần cài .NET
# Nếu vẫn lỗi, thử chạy lại lệnh xattr
```

## 📱 Tính năng

✅ Đăng nhập API Phoenix Logistics  
✅ Quét mã vạch đơn hàng  
✅ Tải và in label tự động  
✅ **Save as PDF** - Lưu vào thư mục Downloads  
✅ Chọn máy in từ danh sách  
✅ Hỗ trợ CUPS printing system  

## 🎯 Cách sử dụng

1. **Mở ứng dụng**
2. **Đăng nhập** (credentials đã được set sẵn)
3. **Chọn máy in** hoặc "📂 Save as PDF"
4. **Quét mã vạch** đơn hàng
5. Label tự động in hoặc lưu vào Downloads

## 📞 Hỗ trợ

- Email: support@phoenixlogistics.vn
- Website: https://phoenixlogistics.vn

## 📝 Lưu ý

- File executable và tất cả `.dylib` phải ở cùng thư mục
- Lần đầu chạy có thể mất vài giây để khởi động
- Nếu dùng "Save as PDF", file sẽ lưu vào `~/Downloads/`
- Khi in, đảm bảo máy in đã bật và online

---

**Copyright © 2025 Phoenix Logistics**
