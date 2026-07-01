# Social App Backend

Hệ thống Backend cho Social App được xây dựng dựa trên kiến trúc **Microservices** kết hợp **Clean Architecture** và **CQRS** pattern.

## 🏗 Tổng quan Kiến trúc (Architecture Overview)

- **Ngôn ngữ & Framework:** .NET 8.0, ASP.NET Core Web API
- **API Gateway:** YARP (Yet Another Reverse Proxy)
- **Message Broker:** RabbitMQ (Giao tiếp bất đồng bộ qua Domain Events)
- **Real-time:** SignalR (Sử dụng trong ChatService)
- **Database:** PostgreSQL (Mỗi service sử dụng một Database độc lập để đảm bảo tính phân tán)
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
6. **Shared:** Class library chứa các thành phần dùng chung (BaseEntity, IRepository, MessageBroker helper) để tránh duplicate code giữa các Service.

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
- Mở thư mục `Domain/Entities`, tạo hoặc sửa class thực thể (Kế thừa `BaseEntity` từ project Shared).
- Định nghĩa các interface cho Repository (Ví dụ: `IUserRepository`) trong `Domain/Repositories`.
- *Lưu ý: Layer này không được phép tham chiếu tới bất kỳ layer nào khác hay thư viện bên ngoài (trừ Shared).*

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

1. **Khởi chạy Infrastructure (RabbitMQ & PostgreSQL):**
   - Đảm bảo bạn đã cài đặt hoặc chạy RabbitMQ và PostgreSQL trên máy (hoặc qua Docker).
   - Kiểm tra và đổi connection string trong `appsettings.json` của từng service nếu cần.

2. **Cấu hình Cổng (Ports):**
   Mỗi Service được cấu hình chạy ở một port cố định để ApiGateway có thể định tuyến chính xác. Cụ thể:
   - **ApiGateway**: `https://localhost:5001` (HTTP: `5000`)
   - **AuthService**: `https://localhost:5101` (HTTP: `5201`)
   - **UserService**: `https://localhost:5102` (HTTP: `5202`)
   - **ChatService**: `https://localhost:5103` (HTTP: `5203`)
   - **PostService**: `https://localhost:5104` (HTTP: `5204`)

3. **Chạy các Services & ApiGateway:**
   Sử dụng Visual Studio (chế độ Multiple Startup Projects) hoặc chạy qua CLI lần lượt cho tất cả các folder Service và ApiGateway:
   ```bash
   cd ApiGateway && dotnet run
   cd Services/AuthService && dotnet run
   cd Services/UserService && dotnet run
   cd Services/ChatService && dotnet run
   cd Services/PostService && dotnet run
   ```
   *(Lưu ý: Nếu bạn chạy lệnh `dotnet run` mà không chỉ định profile, .NET sẽ mặc định chạy bằng HTTP profile với các cổng `5000` và `520x` như đã thiết lập.)*

4. **Cập nhật Database (EF Core Migrations):**
   Trong từng thư mục Service (ngoại trừ Shared và ApiGateway), chạy:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Truy cập:**
   Truy cập Swagger UI tổng hợp thông qua cổng của **ApiGateway**:
   👉 **http://localhost:5000/swagger** (hoặc HTTPS: **https://localhost:5001/swagger**)

   > **💡 Mẹo nhỏ (Quan trọng):**
   > Khi mở trang Swagger, mặc định bạn sẽ chỉ thấy các API của `AuthService`. Để xem API của các service khác (`UserService`, `ChatService`, `PostService`), hãy bấm vào menu thả xuống **"Select a definition"** ở góc trên cùng bên phải màn hình.
