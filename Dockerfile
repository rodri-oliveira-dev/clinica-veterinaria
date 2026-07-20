FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ClinicaVeterinaria.slnx ./
COPY src/Apps/PetShop.Api/PetShop.Api.csproj src/Apps/PetShop.Api/
COPY src/BuildingBlocks/PetShop.Observability/PetShop.Observability.csproj src/BuildingBlocks/PetShop.Observability/
COPY src/BuildingBlocks/PetShop.Observability.AspNetCore/PetShop.Observability.AspNetCore.csproj src/BuildingBlocks/PetShop.Observability.AspNetCore/

RUN dotnet restore ./src/Apps/PetShop.Api/PetShop.Api.csproj

COPY src/ ./src/

RUN dotnet publish ./src/Apps/PetShop.Api/PetShop.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
LABEL org.opencontainers.image.title="PetShop.Api" \
      org.opencontainers.image.description="ASP.NET Core API do monolito modular PetShop." \
      org.opencontainers.image.vendor="Clinica Veterinaria" \
      org.opencontainers.image.licenses="UNLICENSED"

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "PetShop.Api.dll"]
