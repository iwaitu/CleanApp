## CleanApp — ASP.NET Core Clean Architecture 模板

一个基于 **ASP.NET Core 10** 的后端项目模板，采用典型的 **Clean Architecture / 分层结构**，集成：

- PostgreSQL + Entity Framework Core + Identity 用户体系
- JWT 认证与角色授权（默认内置 Admin 种子用户逻辑）
- Swagger / OpenAPI 文档
- Redis 缓存（StackExchange.Redis）
- MongoDB 文件存储接口（`IMongoFileService`，便于扩展 GridFS 或第三方对象存储）

可直接作为你后续项目的起点或 GitHub Template 使用。

> ⚠️ **重要提示：首次使用本模板前，请务必先修改配置文件**
>
> - 更新 `src/CleanApp/appsettings.json` 与 `appsettings.Development.json` 中的：
>   - `ConnectionStrings:DefaultConnection`（PostgreSQL 连接字符串）
>   - `Token:*`（Issuer / Audience / SecretKey 等 JWT 配置）
>   - `Redis:*`（Redis 连接信息，确保与你本机或服务器的 Redis 实例一致）
>   - MongoDB 相关配置（如连接字符串、数据库/集合名等，用于 `IMongoFileService` 文件存储实现）
> - 建议使用 User Secrets 或环境变量管理生产环境敏感信息，避免直接提交到 Git。

---

## 功能概览

- 用户注册 / 登录 / 当前用户信息：`AuthController`
	- `POST /api/auth/register` 注册
	- `POST /api/auth/login` 登录并返回 JWT Token
	- `GET /api/auth/me` 获取当前登录用户信息
- 基础用户领域模型：`AppUser` 继承自 `IdentityUser`
- 文件领域模型与服务接口：
	- `AppFile`：文件领域实体
	- `IFileService`：文件上传 / 下载 / 列表 / 删除
	- `IMongoFileService`：基于 MongoDB 的文件存储接口扩展点
- 基础设施层：
	- `AppDbContext`：基于 `IdentityDbContext<AppUser, IdentityRole, string>` 的 EF Core 上下文
	- `SeedData`：种子数据（Admin 角色 + 管理员账号）
- 统一服务默认配置项目：`CleanApp.ServiceDefaults`
- 独立宿主项目：`CleanApp.AppHost`（如需做多服务组合或统一入口可以扩展）

---

## 解决方案结构

> 根目录为 `CleanApp`，核心代码位于 `src/` 目录

```text
CleanApp.slnx             # 顶层解决方案（VS 2022+）
src/
	src.sln                 # 代码层解决方案
	CleanApp/               # Web API 主项目（API / 认证 / 启动）
	CleanApp.AppHost/       # 宿主项目（可选，用于组合/部署）
	CleanApp.Contracts/     # DTO、分页模型等跨层契约
	CleanApp.Core/          # Core 业务逻辑与接口定义
	CleanApp.Domain/        # 纯领域模型（实体）
	CleanApp.Infrastructure/# 基础设施实现（EF Core、UoW、Seed）
	CleanApp.ServiceDefaults/# 公共服务配置（扩展方法等）
test/
	CleanAppCoreTests/      # 核心层单元测试
```

各项目职责简述：

- `CleanApp`：
	- ASP.NET Core Web API 入口
	- 配置 DI、EF Core、Identity、JWT、Redis、Swagger 等
	- 控制器层 (`Controllers/`)
- `CleanApp.Domain`：
	- 纯 POCO 领域模型（如 `AppUser`, `AppFile`, `BaseEntity`）
	- 不依赖任何基础设施实现
- `CleanApp.Core`：
	- 核心业务接口（`IFileService`, `IMongoFileService` 等）
	- 业务服务实现（`Services/`）
- `CleanApp.Infrastructure`：
	- EF Core `AppDbContext`
	- 迁移、仓储/工作单元 `IUnitOfWork`/`UnitOfWork`
	- 初始种子数据 `SeedData`
- `CleanApp.Contracts`：
	- DTO / 分页模型（例如 `PageOf<T>`）
- `CleanApp.AppHost`：
	- 可用于未来承载多服务、Worker、定时任务等
- `CleanApp.ServiceDefaults`：
	- 统一服务默认配置扩展（`AddServiceDefaults`、`MapDefaultEndpoints` 等）

---

## 技术栈

- **运行时 / 平台**
	- .NET 10 (`net10.0`)
	- ASP.NET Core Web API

- **身份与授权**
	- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
	- JWT Bearer 认证 (`Microsoft.AspNetCore.Authentication.JwtBearer`)

- **数据访问**
	- EF Core + PostgreSQL：`Npgsql.EntityFrameworkCore.PostgreSQL`
	- 迁移工具：`Microsoft.EntityFrameworkCore.Tools`

- **缓存与消息**
	- Redis 缓存：`StackExchange.Redis` + `Microsoft.Extensions.Caching.StackExchangeRedis`
	- SignalR + Redis 背板：`Microsoft.AspNetCore.SignalR.StackExchangeRedis`

- **API 文档**
	- OpenAPI / Swagger：`Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore`

- **其他**
	- Docker 支持 (`Dockerfile` + `Microsoft.VisualStudio.Azure.Containers.Tools.Targets`)

---

## 快速开始

### 1. 准备环境

本模板假定你已安装：

- .NET SDK 10.x（或与 `TargetFramework` 匹配的版本）
- PostgreSQL 数据库实例
- Redis 服务
- （可选）MongoDB（如果你要落地 `IMongoFileService`）

### 2. 克隆项目

```bash
git clone https://github.com/your-account/CleanApp.git
cd CleanApp
```

或者在 GitHub 上直接使用 **Use this template** 创建新仓库。

### 3. 配置应用设置

核心配置位于 `src/CleanApp/appsettings.json` 及 `appsettings.Development.json`，重点包括：

- 数据库连接：`ConnectionStrings:DefaultConnection`
- JWT 配置：`Token:Issuer` / `Token:Audience` / `Token:SecretKey`
- Redis：`Redis:EndPoints` / `Redis:Password`
- Mongo（如果使用）：自定义在 `IMongoFileService` 实现中读取

示例（请根据实际修改）：

```jsonc
{
	"ConnectionStrings": {
		"DefaultConnection": "Host=localhost;Port=5432;Database=cleanapp_db;Username=postgres;Password=your_password"
	},
	"Token": {
		"Issuer": "cleanapp",
		"Audience": "cleanapp-client",
		"SecretKey": "your_long_secret_key_here"
	},
	"Redis": {
		"EndPoints": "localhost:6379",
		"Password": ""
	}
}
```

> 注意：生产环境请使用 **User Secrets** 或环境变量来管理敏感信息。

### 4. 运行数据库迁移与种子数据

项目在启动时，会在 `Program.cs` 中执行：

- `db.Database.Migrate()` 自动迁移
- `SeedData.Initialize(services)` 创建默认角色与管理员用户

请在 `SeedData` 中替换默认管理员密码：

```csharp
string password = "place_your_password_here"; // 修改为你的安全密码
```

### 5. 运行项目

在解决方案目录执行：

```bash
cd src
dotnet restore
dotnet run --project CleanApp/CleanApp.csproj
```

默认情况下：

- API 监听在 `https://localhost:xxxx`（具体端口由 `launchSettings.json` 或 Kestrel 决定）
- Swagger UI 地址：`/swagger`

---

## 主要功能说明

### AuthController（认证接口）

位置：`src/CleanApp/Controllers/AuthController.cs`

- `POST /api/auth/register`
	- 请求体：`{ "email": "user@example.com", "password": "Pa$$w0rd" }`
	- 作用：创建一个新用户（使用 Identity）

- `POST /api/auth/login`
	- 请求体：同上
	- 作用：校验邮箱+密码，成功后返回 JWT Token

- `GET /api/auth/me`
	- 请求头：`Authorization: Bearer <token>`
	- 作用：返回当前登录用户的基础信息（Id / Email）

### 文件服务接口

核心接口定义在 `CleanApp.Core`：

- `IFileService`
	- `UploadAsync(Stream fileStream, string fileName)`
	- `DownloadAsync(string id)`
	- `DeleteAsync(string id)`
	- `ListAsync(string? name, int page = 1, int pageSize = 20)` 返回 `PageOf<AppFile>`

- `IMongoFileService`
	- `UploadFileAsync`
	- `DownloadFileAsync`
	- `DeleteFileAsync`

默认已在 `Program.cs` 中注册：

```csharp
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IMongoFileService>(sp => new MongoFileService(config));
```

你可以在 `CleanApp.Core/Services` 中扩展 `FileService` 实现，以及在 `MongoFileService` 中对接 Mongo GridFS 或自定义文件存储。

---

## 如何将本仓库作为模板使用

### 1. 在 GitHub 上设置为 Template

1. 将本项目推送到你的 GitHub：
	 ```bash
	 git remote add origin git@github.com:your-account/cleanapp-template.git
	 git push -u origin main
	 ```
2. 在仓库 Settings 中勾选 **Template repository**。
3. 之后即可在 GitHub 上点击 **Use this template** 创建新项目。

### 2. 基于模板创建新项目时建议修改

- 修改解决方案名 / 项目名（可全局替换 `CleanApp`）
- 更新 `SeedData` 中的默认管理员邮箱 / 密码
- 调整领域模型（`CleanApp.Domain`）以适配你的业务
- 根据需要增加新的模块项目（例如 `YourApp.BackgroundWorker`）

---

## 运行测试

已有基础单元测试项目：`test/CleanAppCoreTests`。

在 `src` 同级目录执行：

```bash
dotnet test
```

你可以在 `CleanAppCoreTests` 中为核心业务逻辑补充更多用例。

---

## 典型扩展点

- **认证 / 授权**：
	- 新增角色 / 权限模型，配合 `[Authorize(Roles = "Admin")]` 等装饰器
- **领域模型**：
	- 在 `CleanApp.Domain` 中添加更多实体，并通过 `AppDbContext` 映射
- **应用服务**：
	- 在 `CleanApp.Core/Services` 中添加业务服务，并在 `Program.cs` 中注册 DI
- **基础设施**：
	- 拓展不同数据库 / 存储实现（如多租户、读写分离等）
- **API 网关 / 前端**：
	- 基于此模板扩展 BFF、前端项目或 API 网关

---

## 许可证

> 如果你要将其作为公共模板使用，建议在仓库根目录添加一个明确的 LICENSE 文件（如 MIT、Apache-2.0 等），并在此处说明。

此处暂留空，由使用者根据自身需求补充。
