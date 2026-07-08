# ============================================================
# stop-all.ps1 — Dừng toàn bộ services của Social App
# ============================================================
# Script này sẽ tìm và kill tất cả các process dotnet
# đang chạy cho các service của Social App.
# ============================================================

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Social App — Stopping All Services...  " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$serviceNames = @("ApiGateway", "AuthService", "UserService", "ChatService", "PostService")
$killed = 0

# Tìm các process dotnet đang chạy liên quan đến các service
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    foreach ($proc in $dotnetProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)" -ErrorAction SilentlyContinue).CommandLine
            
            foreach ($svcName in $serviceNames) {
                if ($cmdLine -and $cmdLine -like "*$svcName*") {
                    Write-Host "[STOP] Dang dung $svcName (PID: $($proc.Id))..." -ForegroundColor Yellow
                    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
                    $killed++
                    break
                }
            }
        }
        catch {
            # Bỏ qua lỗi nếu process đã kết thúc
        }
    }
}

if ($killed -eq 0) {
    Write-Host "[INFO] Khong tim thay service nao dang chay." -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "[DONE] Da dung $killed service(s)." -ForegroundColor Green
}

Write-Host ""
