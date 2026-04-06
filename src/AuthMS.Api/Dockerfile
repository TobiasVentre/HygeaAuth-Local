FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/AuthMS.Api/AuthMS.Api.csproj", "src/AuthMS.Api/"]
COPY ["src/AuthMS.Application/AuthMS.Application.csproj", "src/AuthMS.Application/"]
COPY ["src/AuthMS.Domain/AuthMS.Domain.csproj", "src/AuthMS.Domain/"]
COPY ["src/AuthMS.Infrastructure/AuthMS.Infrastructure.csproj", "src/AuthMS.Infrastructure/"]
RUN dotnet restore "src/AuthMS.Api/AuthMS.Api.csproj"

COPY . .
WORKDIR "/src/src/AuthMS.Api"
RUN dotnet build "AuthMS.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthMS.Api.dll"]
