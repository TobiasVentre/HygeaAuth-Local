# Etapa base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080


# Etapa build (compilaci�n)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY ["AuthMS/AuthMS.csproj", "AuthMS/"]
RUN dotnet restore "AuthMS/AuthMS.csproj"



COPY AuthMS/AuthMS.csproj AuthMS/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore "AuthMS/AuthMS.csproj"

# Copiar todo el c�digo y compilar
COPY . .
WORKDIR "/src/AuthMS"
RUN dotnet build "AuthMS.csproj" -c Release -o /app/build


# Etapa de publicaci�n
FROM build AS publish
RUN dotnet publish "AuthMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthMS.dll"]
