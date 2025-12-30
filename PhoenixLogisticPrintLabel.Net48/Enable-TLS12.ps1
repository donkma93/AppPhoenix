# Enable TLS 1.2 on Windows 7
# Run with: powershell -ExecutionPolicy Bypass -File Enable-TLS12.ps1

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "FIX TLS 1.2 CHO WINDOWS 7" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "KHONG CO QUYEN ADMINISTRATOR!" -ForegroundColor Red
    Write-Host "Click chuot phai vao file nay va chon 'Run with PowerShell'" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "HOAC chay lenh:" -ForegroundColor Yellow
    Write-Host "Start-Process powershell -Verb RunAs -ArgumentList '-ExecutionPolicy Bypass -File `"$PSCommandPath`"'" -ForegroundColor Green
    Write-Host ""
    pause
    exit 1
}

Write-Host "Dang apply registry settings..." -ForegroundColor Green
Write-Host ""

try {
    # Enable TLS 1.2 Client
    New-Item -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Force | Out-Null
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Name "DisabledByDefault" -Value 0 -Type DWord
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Name "Enabled" -Value 1 -Type DWord
    Write-Host "[OK] TLS 1.2 Client enabled" -ForegroundColor Green

    # Enable TLS 1.2 Server
    New-Item -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server" -Force | Out-Null
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server" -Name "DisabledByDefault" -Value 0 -Type DWord
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server" -Name "Enabled" -Value 1 -Type DWord
    Write-Host "[OK] TLS 1.2 Server enabled" -ForegroundColor Green

    # Enable strong crypto for .NET Framework
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\.NETFramework\v4.0.30319" -Name "SchUseStrongCrypto" -Value 1 -Type DWord
    Write-Host "[OK] .NET Framework strong crypto enabled" -ForegroundColor Green

    if (Test-Path "HKLM:\SOFTWARE\Wow6432Node") {
        Set-ItemProperty -Path "HKLM:\SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319" -Name "SchUseStrongCrypto" -Value 1 -Type DWord
        Write-Host "[OK] .NET Framework strong crypto enabled (WOW64)" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "HOAN THANH!" -ForegroundColor Green
    Write-Host "BAT BUOC PHAI KHOI DONG LAI MAY!" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    
    $restart = Read-Host "Ban co muon khoi dong lai may bay gio khong? (Y/N)"
    if ($restart -eq "Y" -or $restart -eq "y") {
        Write-Host "Dang khoi dong lai..." -ForegroundColor Yellow
        Restart-Computer -Force
    }
}
catch {
    Write-Host ""
    Write-Host "LOI: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}

Write-Host ""
pause
