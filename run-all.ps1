# ============================================================
# run-all.ps1 — Khởi chạy toàn bộ services của Social App
# ============================================================
# Mỗi service sẽ được chạy trong một cửa sổ PowerShell riêng.
# Để dừng tất cả, đóng các cửa sổ hoặc nhấn Ctrl+C trong từng cửa sổ.
# ============================================================

$root = $PSScriptRoot

# Danh sách các service cần chạy: [Tên hiển thị, Đường dẫn tương đối]
$services = @(
    @{ Name = "ApiGateway";  Path = "ApiGateway" },
    @{ Name = "AuthService"; Path = "Services\AuthService" },
    @{ Name = "UserService"; Path = "Services\UserService" },
    @{ Name = "ChatService"; Path = "Services\ChatService" },
    @{ Name = "PostService"; Path = "Services\PostService" }
)

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Social App — Starting All Services...  " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

foreach ($svc in $services) {
    $projectPath = Join-Path $root $svc.Path
    $name = $svc.Name

    if (-Not (Test-Path $projectPath)) {
        Write-Host "[ERROR] Khong tim thay thu muc: $projectPath" -ForegroundColor Red
        continue
    }

    Write-Host "[START] $name => $projectPath" -ForegroundColor Green

    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$projectPath'; Write-Host '>>> Running $name ...' -ForegroundColor Yellow; dotnet run"
    )

    # Đợi 1 giây giữa mỗi service để tránh xung đột port khi khởi động
    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Tat ca services da duoc khoi chay!     " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ports:" -ForegroundColor Yellow
Write-Host "  ApiGateway  : http://localhost:5000 | https://localhost:5001" 
Write-Host "  AuthService : http://localhost:5201 | https://localhost:5101"
Write-Host "  UserService : http://localhost:5202 | https://localhost:5102"
Write-Host "  ChatService : http://localhost:5203 | https://localhost:5103"
Write-Host "  PostService : http://localhost:5204 | https://localhost:5104"
Write-Host ""
Write-Host "Swagger UI: http://localhost:5000/swagger" -ForegroundColor Green
Write-Host ""
