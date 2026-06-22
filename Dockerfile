
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/RegistrationMonitor.Core/*.csproj src/RegistrationMonitor.Core/
COPY src/RegistrationMonitor.Infrastructure/*.csproj src/RegistrationMonitor.Infrastructure/
COPY src/RegistrationMonitor.Worker/*.csproj src/RegistrationMonitor.Worker/

RUN dotnet restore src/RegistrationMonitor.Worker/RegistrationMonitor.Worker.csproj

COPY src/ src/

RUN dotnet publish src/RegistrationMonitor.Worker/RegistrationMonitor.Worker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends tzdata && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "RegistrationMonitor.Worker.dll"]
