# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/LeasiNetWeb.Web/LeasiNetWeb.Web.csproj \
    -c Release \
    -o /app/out \
    --no-self-contained

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
COPY --from=build /src/README.md /app/README.md

# Disable the .NET diagnostics server.
# Railway's seccomp profile prevents the DependencyInjectionEventSource static
# initializer from completing when built via Nixpacks; the official aspnet image
# handles this correctly, but we set the flag as defence-in-depth.
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "LeasiNetWeb.Web.dll"]
