# HƯỚNG DẪN CÀI ĐÁT - PHOENIX LOGISTIC PRINT LABEL (WINDOWS 7)

## 📌 GIỚI THIỆU

Đây là phiên bản **đặc biệt cho Windows 7** của ứng dụng in nhãn Phoenix Logistics, đã được tối ưu để khắc phục lỗi SSL/TLS.

---

## ⚠️ YÊU CẦU HỆ THỐNG

- **Windows 7 Service Pack 1** (bắt buộc)
- **.NET Framework 4.8**
- Kết nối Internet
- Máy in (nếu muốn in trực tiếp)

---

## 🔧 BƯỚC 1: CÀI ĐẶT .NET FRAMEWORK 4.8

1. **Kiểm tra đã cài chưa:**
   - Mở `Control Panel` → `Programs and Features`
   - Tìm "Microsoft .NET Framework 4.8" trong danh sách
   
2. **Nếu chưa có, tải về và cài đặt:**
   - Truy cập: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Tải file `ndp48-web.exe` hoặc `ndp48-x86-x64-allos-enu.exe`
   - Chạy file cài đặt
   - **KHỞI ĐỘNG LẠI MÁY TÍNH**

---

## 🔐 BƯỚC 2: BẬT HỖ TRỢ TLS 1.2 (QUAN TRỌNG!)

### ⚡ CÁCH 1: DÙNG FILE REGISTRY (NHANH NHẤT)

1. Tìm file **`enable-tls12.reg`** trong thư mục ứng dụng
2. **Click phải** vào file → chọn **"Merge"** (hoặc double-click)
3. Nhấn **"Yes"** để xác nhận
4. Nhấn **"OK"** khi thấy thông báo thành công
5. **KHỞI ĐỘNG LẠI MÁY TÍNH**

### 🛠️ CÁCH 2: CHỈNH REGISTRY THỦ CÔNG

1. Nhấn **Win + R**, gõ `regedit`, nhấn **Enter**
2. Vào đường dẫn:
   ```
   HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols
   ```
3. **Click phải** vào `Protocols` → **New** → **Key** → đặt tên `TLS 1.2`
4. **Click phải** vào `TLS 1.2` → **New** → **Key** → đặt tên `Client`
5. **Click phải** vào `TLS 1.2` → **New** → **Key** → đặt tên `Server`
6. Trong thư mục `Client`:
   - Click phải → **New** → **DWORD (32-bit) Value** → đặt tên `DisabledByDefault`
   - Double-click → nhập giá trị `0`
   - Click phải → **New** → **DWORD (32-bit) Value** → đặt tên `Enabled`
   - Double-click → nhập giá trị `1`
7. **Lặp lại bước 6** cho thư mục `Server`
8. **KHỞI ĐỘNG LẠI MÁY TÍNH**

---

## 🚀 BƯỚC 3: CHẠY ỨNG DỤNG

1. Mở thư mục: `PhoenixLogisticPrintLabel.Net48\bin\Release\net48\`
2. Double-click file: **`PhoenixLogisticPrintLabel.Net48.exe`**
3. Ứng dụng sẽ tự động đăng nhập với tài khoản mặc định
4. Quét mã vạch để in nhãn

---

## ❌ XỬ LÝ LỖI

### Lỗi: "Could not create SSL/TLS secure channel"

**Nguyên nhân:** Chưa bật TLS 1.2

**Giải pháp:**
1. Thực hiện BƯỚC 2 ở trên
2. **KHỞI ĐỘNG LẠI MÁY TÍNH** (bắt buộc!)
3. Chạy lại ứng dụng

---

### Lỗi: "This application requires .NET Framework 4.8"

**Nguyên nhân:** Chưa cài .NET Framework 4.8

**Giải pháp:** Thực hiện BƯỚC 1 ở trên

---

### Lỗi: "Không thể kết nối đến server"

**Kiểm tra:**
- ✅ Kết nối Internet hoạt động bình thường
- ✅ Đã khởi động lại máy sau khi bật TLS 1.2
- ✅ Tường lửa (firewall) không chặn ứng dụng

---

## 📝 LƯU Ý

- Windows 7 đã **hết hỗ trợ** từ Microsoft (01/2020)
- Khuyến nghị **nâng cấp lên Windows 10/11** để đảm bảo bảo mật
- Nếu có thể, sử dụng phiên bản mới hơn trên Windows 10/11

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề, vui lòng liên hệ bộ phận IT của Phoenix Logistics.

---

**Phiên bản:** 1.0.0  
**Ngày phát hành:** 30/12/2025  
**Tương thích:** Windows 7 SP1 trở lên
