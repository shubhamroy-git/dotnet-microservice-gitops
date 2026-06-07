# ==========================================
# STAGE 1: Build & Compile (Heavy SDK Image)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Optimize Layer Caching: Copy only the project file first
COPY src/ProductCatalogApi/ProductCatalogApi.csproj ./src/ProductCatalogApi/
RUN dotnet restore ./src/ProductCatalogApi/ProductCatalogApi.csproj

# Copy the rest of the source code and build production-ready release binaries
COPY src/ProductCatalogApi/ ./src/ProductCatalogApi/
WORKDIR /app/src/ProductCatalogApi
RUN dotnet publish -c Release -o /app/publish --no-restore

# ==========================================
# STAGE 2: Secure Production Runtime (Chiseled Image)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime-env
WORKDIR /app

# Copy the pre-compiled binaries from Stage 1
COPY --from=build-env /app/publish .

# Expose the standard non-privileged port used by modern .NET container images
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Run the app using the built-in non-root user (app) provided by chiseled base images
ENTRYPOINT ["dotnet", "ProductCatalogApi.dll"]