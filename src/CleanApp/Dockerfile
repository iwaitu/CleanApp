# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CleanApp/CleanApp.csproj", "CleanApp/"]
COPY ["CleanApp.Core/CleanApp.Core.csproj", "CleanApp.Core/"]
COPY ["CleanApp.Domain/CleanApp.Domain.csproj", "CleanApp.Domain/"]
COPY ["CleanApp.Contracts/CleanApp.Contracts.csproj", "CleanApp.Contracts/"]
COPY ["CleanApp.Infrastructure/CleanApp.Infrastructure.csproj", "CleanApp.Infrastructure/"]
RUN dotnet restore "./CleanApp/CleanApp.csproj"
COPY . .
WORKDIR "/src/CleanApp"
RUN dotnet build "./CleanApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanApp.dll"]