FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AuthMS.Api/AuthMS.Api.csproj", "AuthMS.Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "AuthMS.Api/AuthMS.Api.csproj"

COPY . .
WORKDIR "/src/AuthMS.Api"
RUN dotnet build "AuthMS.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthMS.Api.dll"]
