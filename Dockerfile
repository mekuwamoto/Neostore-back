FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Neostore.Api/Neostore.Api.csproj", "Neostore.Api/"]
COPY ["src/Neostore.Application/Neostore.Application.csproj", "Neostore.Application/"]
COPY ["src/Neostore.Domain/Neostore.Domain.csproj", "Neostore.Domain/"]
COPY ["src/Neostore.Infrastructure/Neostore.Infrastructure.csproj", "Neostore.Infrastructure/"]
COPY ["src/Neostore.Persistence/Neostore.Persistence.csproj", "Neostore.Persistence/"]
RUN dotnet restore "Neostore.Api/Neostore.Api.csproj"
COPY src/ .
WORKDIR "/src/Neostore.Api"
RUN dotnet build "Neostore.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Neostore.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neostore.Api.dll"]
