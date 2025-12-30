# Phoenix Logistic Print Label - Windows 7 Edition

## Giới thiệu

Đây là phiên bản đặc biệt của ứng dụng Phoenix Logistic Print Label được tối ưu cho **Windows 7**.

## Yêu cầu hệ thống

- **Windows 7 Service Pack 1** trở lên
- **.NET Framework 4.8** (tải tại: https://dotnet.microsoft.com/download/dotnet-framework/net48)
- Đã cập nhật Windows Update

## Hướng dẫn cài đặt cho Windows 7

### Bước 1: Cài đặt .NET Framework 4.8

1. Truy cập: https://dotnet.microsoft.com/download/dotnet-framework/net48
2. Tải và cài đặt file cài đặt
3. Khởi động lại máy tính

### Bước 2: Bật hỗ trợ TLS 1.2 (QUAN TRỌNG!)

Windows 7 mặc định không bật TLS 1.2, cần bật thủ công qua Registry:

#### Cách 1: Sử dụng file Registry (Khuyến nghị)

1. Tạo file mới có tên `enable-tls12.reg`
2. Copy nội dung sau vào file:

```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2]

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client]
"DisabledByDefault"=dword:00000000
"Enabled"=dword:00000001

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server]
"DisabledByDefault"=dword:00000000
"Enabled"=dword:00000001
```

3. Double-click file `enable-tls12.reg` để import vào Registry
4. Nhấn Yes để xác nhận
5. **Khởi động lại máy tính**

#### Cách 2: Chỉnh sửa Registry thủ công

1. Mở Registry Editor (Win + R, gõ `regedit`)
2. Đi đến: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols`
3. Tạo key mới tên `TLS 1.2` (nếu chưa có)
4. Trong `TLS 1.2`, tạo 2 key con: `Client` và `Server`
5. Trong mỗi key `Client` và `Server`, tạo 2 giá trị DWORD:
   - `DisabledByDefault` = `0`
   - `Enabled` = `1`
6. **Khởi động lại máy tính**

### Bước 3: Chạy ứng dụng

1. Mở file `PhoenixLogisticPrintLabel.Net48.exe`
2. Nếu xuất hiện lỗi SSL/TLS, kiểm tra lại Bước 2

## Sự khác biệt với phiên bản cũ

- ✅ Hỗ trợ Windows 7
- ✅ Tự động bật TLS 1.2 khi khởi động app
- ✅ Thông báo lỗi SSL/TLS rõ ràng hơn
- ✅ Tất cả chức năng tương tự phiên bản .NET 9.0

## Xử lý lỗi thường gặp

### Lỗi: "The request was aborted: Could not create SSL/TLS secure channel"

**Nguyên nhân:** Windows 7 chưa bật TLS 1.2

**Giải pháp:** Thực hiện Bước 2 ở trên (Bật TLS 1.2) và khởi động lại máy

### Lỗi: "This application requires .NET Framework 4.8"

**Nguyên nhân:** Chưa cài .NET Framework 4.8

**Giải pháp:** Thực hiện Bước 1 ở trên

## Build từ source code

Nếu bạn muốn build từ source code:

```powershell
cd PhoenixLogisticPrintLabel.Net48
dotnet restore
msbuild PhoenixLogisticPrintLabel.Net48.csproj /p:Configuration=Release
```

Hoặc dùng Visual Studio 2019/2022:
1. Mở file `.csproj`
2. Build > Build Solution (Ctrl + Shift + B)

## Hỗ trợ

Nếu gặp vấn đề, vui lòng liên hệ bộ phận IT của Phoenix Logistics.

---

**Lưu ý:** Windows 7 đã hết hỗ trợ từ Microsoft. Khuyến nghị nâng cấp lên Windows 10/11 để được bảo mật tốt hơn.
