# Social App Backend

Hệ thống Backend cho Social App được xây dựng dựa trên kiến trúc **Microservices** kết hợp **Clean Architecture** và **CQRS** pattern.

## 🏗 Tổng quan Kiến trúc (Architecture Overview)

- **Ngôn ngữ & Framework:** .NET 8.0, ASP.NET Core Web API
- **API Gateway:** YARP (Yet Another Reverse Proxy)
- **Message Broker:** RabbitMQ (Giao tiếp bất đồng bộ qua Domain Events)
- **Real-time:** SignalR (Sử dụng trong ChatService)
- **Database:** PostgreSQL (Mỗi service sử dụng một Database độc lập để đảm bảo tính phân tán)
- **Containerization:** Docker & Docker Compose
- **Design Pattern:** Clean Architecture, CQRS (MediatR), Repository Pattern

---

## 📦 Cấu trúc Services

Hệ thống bao gồm các Service độc lập sau:

1. **ApiGateway:** 
   - Điểm vào (Entry-point) duy nhất cho toàn bộ hệ thống từ Client.
   - Routing các request đến đúng Service tương ứng.
   - Tổng hợp tài liệu Swagger (Swagger UI Aggregation).
2. **AuthService:** Quản lý đăng ký, đăng nhập (JWT), cấp phát Refresh Token.
3. **UserService:** Quản lý thông tin profile người dùng (Tên, tuổi, avatar...).
4. **ChatService:** Xử lý nhắn tin (1-1 và Group Chat) theo thời gian thực với SignalR.
5. **PostService:** Quản lý bài viết (Tạo bài, feed, phân quyền hiển thị).

---

## 📁 Cấu trúc thư mục

```
Backend/
├── ApiGateway/                    # YARP Reverse Proxy Gateway
│   ├── Dockerfile
│   ├── Program.cs
│   └── appsettings.json           # Route & Cluster config
├── Services/
│   ├── AuthService/               # Xác thực & JWT
│   │   ├── Api/Controllers/
│   │   ├── Application/           # Commands, Queries, Handlers, DTOs
│   │   ├── Domain/                # Entities, Repositories (Interface)
│   │   ├── Infrastructure/        # DbContext, Repositories, Messaging
│   │   └── Dockerfile
│   ├── UserService/
│   │   ├── ...                    # Cấu trúc tương tự AuthService
│   │   └── Dockerfile
│   ├── ChatService/
│   │   ├── ...
│   │   ├── Hubs/                  # SignalR Hub
│   │   └── Dockerfile
│   └── PostService/
│       ├── ...
│       └── Dockerfile
├── DatabaseSchemas/               # SQL scripts tạo database
├── scripts/                       # Helper scripts (Docker init)
├── docker-compose.yml             # Orchestrate toàn bộ hệ thống
├── run-all.ps1                    # Khởi chạy local (PowerShell)
├── stop-all.ps1                   # Dừng toàn bộ services local
└── SocialApp.sln
```

---

## 🔄 Flow Hoạt động của hệ thống (Operational Flow)

### 1. Luồng Request đồng bộ (REST API)
- Client gọi một API (VD: `POST /api/v1/post`).
- **ApiGateway** tiếp nhận request, dựa vào route `api/v1/post/*` để forward request tới `PostService` (qua YARP).
- **Controller** của PostService nhận request, mapping Data vào Command/Query (DTO).
- Gửi Command/Query qua **MediatR** (`_mediator.Send(command)`).
- **CommandHandler** xử lý business logic:
  - Lấy/Ghi dữ liệu từ Database thông qua **Repository**.
  - Trả kết quả về cho Controller.
- Controller trả HTTP Response qua ApiGateway về cho Client.

### 2. Luồng giao tiếp bất đồng bộ (Asynchronous / Event-Driven)
Khi một service thực hiện thay đổi dữ liệu quan trọng, nó sẽ phát (Publish) một Event qua **RabbitMQ**. Các service khác có thể lắng nghe (Subscribe) để cập nhật dữ liệu của mình.
- **Ví dụ:** Khi `AuthService` đăng ký user thành công, nó sẽ publish event `auth.user.registered`.
- `UserService` lắng nghe event này và tự động tạo một `UserProfile` rỗng với ID tương ứng để quản lý thông tin profile.

### 3. Luồng hoạt động Real-time (SignalR)
Trong `ChatService`:
- Client kết nối websocket trực tiếp thông qua ApiGateway (route `/chatHub`).
- ApiGateway proxy kết nối WebSocket tới ChatService.
- Client gọi phương thức (ví dụ: `SendPrivateMessage`) trên Hub.
- ChatService lưu tin nhắn xuống Database và lập tức đẩy notification (Broadcast) cho Client người nhận (`ReceiveMessage`).

---

## 🛠 Workflow làm việc (Development Workflow)

Mỗi Service đều tuân thủ chặt chẽ **Clean Architecture** chia làm 4 lớp chính. Khi bạn phát triển một tính năng mới trong một Service, hãy thực hiện theo thứ tự sau:

### Bước 1: Domain Layer (Cốt lõi)
Nơi định nghĩa các Entity và Contract (Interface).
- Mở thư mục `Domain/Entities`, tạo hoặc sửa class thực thể.
- Định nghĩa các interface cho Repository (Ví dụ: `IUserRepository`) trong `Domain/Repositories`.
- *Lưu ý: Layer này không được phép tham chiếu tới bất kỳ layer nào khác hay thư viện bên ngoài.*

### Bước 2: Infrastructure Layer (Hạ tầng)
Nơi làm việc với Database và các hệ thống bên ngoài.
- Cấu hình Entity mapping bằng FluentAPI trong `[Service]DbContext.cs` (nằm trong `Infrastructure/Data`).
- Implement các interface của Repository ở bước 1 vào thư mục `Infrastructure/Repositories`.
- Cấu hình DbContext trong `Program.cs`.

### Bước 3: Application Layer (Logic nghiệp vụ)
Áp dụng CQRS pattern.
- Tạo các Record DTO (Data Transfer Object) để nhận/trả dữ liệu trong `Application/DTOs`.
- Định nghĩa Command/Query (`IRequest<TResponse>`) trong `Application/Commands` hoặc `Application/Queries`.
- Tạo các Handler (`IRequestHandler<TCommand, TResponse>`) trong `Application/Handlers`. 
- Nơi đây chứa 100% Business Logic. Handler sẽ gọi các phương thức từ Repository để thao tác dữ liệu.

### Bước 4: Api Layer (Trình diễn)
Nơi tiếp nhận Request HTTP.
- Tạo Controller trong `Api/Controllers`.
- Validate dữ liệu đầu vào.
- Chuyển tiếp payload sang Command/Query và gọi `_mediator.Send()`.
- Trả về HTTP Status Codes phù hợp (Ok, CreatedAtAction, NotFound, BadRequest).

---

## 🚀 Hướng dẫn khởi chạy

### Yêu cầu cài đặt

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Cách 1: Chạy Local (Development — khuyến nghị khi dev)

Chạy PostgreSQL & RabbitMQ bằng Docker, còn các service chạy trực tiếp trên máy (hỗ trợ hot-reload):

```powershell
# 1. Khởi động infrastructure (PostgreSQL + RabbitMQ)
docker-compose up -d postgres rabbitmq

# 2. Chạy tất cả services
.\run-all.ps1

# 3. Dừng tất cả services
.\stop-all.ps1

# 4. Dừng infrastructure
docker-compose down
```

### Cách 2: Chạy toàn bộ bằng Docker Compose (Production-like)

Tất cả services + infrastructure đều chạy trong Docker containers:

```powershell
# Khởi chạy toàn bộ (build & run)
docker-compose up -d --build

# Xem logs
docker-compose logs -f                  # Tất cả
docker-compose logs -f auth-service     # Chỉ AuthService
docker-compose logs -f rabbitmq         # Chỉ RabbitMQ

# Dừng toàn bộ
docker-compose down

# Dừng và xóa toàn bộ data (volumes)
docker-compose down -v
```

### Cấu hình Ports

| Service | HTTP | HTTPS |
|---------|------|-------|
| **ApiGateway** | `http://localhost:5000` | `https://localhost:5001` |
| **AuthService** | `http://localhost:5201` | `https://localhost:5101` |
| **UserService** | `http://localhost:5202` | `https://localhost:5102` |
| **ChatService** | `http://localhost:5203` | `https://localhost:5103` |
| **PostService** | `http://localhost:5204` | `https://localhost:5104` |
| **RabbitMQ Management** | `http://localhost:15672` | — |
| **PostgreSQL** | `localhost:5432` | — |

### Cập nhật Database

Các services đã được cấu hình **auto-migrate** khi khởi động. Ngoài ra bạn cũng có thể:

- **Cách 1 (SQL Scripts có sẵn):** 
  Thực thi các file `.sql` trong thư mục `DatabaseSchemas/` (`auth_db.sql`, `user_db.sql`, `post_db.sql`, `chat_db.sql`) vào các database tương ứng trên PostgreSQL.
- **Cách 2 (EF Core Migrations):**
  ```bash
  cd Services/AuthService
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

---

## 🌐 Truy cập

- **Swagger UI (tổng hợp):** 👉 **http://localhost:5000/swagger**
- **RabbitMQ Management:** 👉 **http://localhost:15672** (user: `guest` / pass: `guest`)

> **💡 Mẹo nhỏ (Quan trọng):**
> Khi mở trang Swagger, mặc định bạn sẽ chỉ thấy các API của `AuthService`. Để xem API của các service khác (`UserService`, `ChatService`, `PostService`), hãy bấm vào menu thả xuống **"Select a definition"** ở góc trên cùng bên phải màn hình.
