# Product Catalog API

Product Catalog API is a small .NET microservice that exposes a few HTTP endpoints for catalog-style data, lightweight system metrics, health checks, and API documentation. It is designed to be run locally during development, containerized for deployment, and paired with a separate GitOps repository that manages Kubernetes manifests.

This repository contains the application code only. The Kubernetes deployment, service, and secret definitions live in the companion repository: [gitops-infra-config](../gitops-infra-config/README.md).

## What this service does

The service is intentionally small, but it covers the main building blocks of a modern microservice:

- Controller-based HTTP APIs
- OpenAPI documentation in development
- Health probes for Kubernetes readiness and liveness
- Containerized execution on port `8080`
- GitOps-friendly deployment artifacts in a separate repo

## API surface

The application currently exposes the following endpoints:

- `GET /api/products` returns a simple list of product names.
- `GET /api/metrics` returns a sample metrics payload.
- `GET /test-ping` returns `Pong!` as a quick verification endpoint.
- `GET /healthz/live` is the liveness probe used by Kubernetes.
- `GET /healthz/ready` is the readiness probe used by Kubernetes.

When the app is running in Development, it also enables:

- OpenAPI generation
- Scalar API documentation UI

## Technology stack

- .NET 10
- ASP.NET Core Web API
- Microsoft.AspNetCore.OpenApi
- Scalar.AspNetCore
- Docker multi-stage build

## Repository layout

```text
dotnet-microservice-gitops/
├── Dockerfile
├── Jenkinsfile
├── src/
│   └── ProductCatalogApi/
│       ├── Controllers/
│       │   ├── MetricsController.cs
│       │   └── ProductsController.cs
│       ├── Program.cs
│       ├── ProductCatalogApi.csproj
│       ├── ProductCatalogApi.http
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Properties/
│           └── launchSettings.json
```

## Local development

### Prerequisites

- .NET SDK 10
- Docker, if you want to build and run the container image

### Run the app locally

From the repository root:

```bash
dotnet restore src/ProductCatalogApi/ProductCatalogApi.csproj
dotnet run --project src/ProductCatalogApi/ProductCatalogApi.csproj
```

By default, the local development profile uses:

- `http://localhost:5154`
- `https://localhost:7104`

### Example requests

```bash
curl http://localhost:5154/api/products
curl http://localhost:5154/api/metrics
curl http://localhost:5154/test-ping
curl http://localhost:5154/healthz/live
curl http://localhost:5154/healthz/ready
```

### OpenAPI and Scalar UI

In Development, the app maps OpenAPI and Scalar UI routes automatically. If you run the application locally with the Development environment, the documentation endpoints are available without additional configuration.

## Container build

The repository includes a multi-stage Dockerfile that builds and publishes the API using a .NET SDK image, then copies the output into a smaller ASP.NET runtime image.

### Build the image

```bash
docker build -t product-catalog-api:local .
```

### Run the image

```bash
docker run --rm -p 8080:8080 product-catalog-api:local
```

The container listens on port `8080` and sets `ASPNETCORE_URLS=http://+:8080`.

## Health checks and Kubernetes readiness

The application publishes separate health probe endpoints so Kubernetes can distinguish between a live process and a ready instance.

- `GET /healthz/live` always returns the liveness state.
- `GET /healthz/ready` always returns the readiness state.

The GitOps manifests use these endpoints for `livenessProbe` and `readinessProbe` respectively.

## CI/CD and GitOps relationship

This repository is the application source of truth. A separate GitOps repository manages the Kubernetes resources that deploy this service.

The flow is:

1. Application code changes land in this repository.
2. The image is built and pushed to a container registry.
3. The GitOps repository updates the deployment manifest to point at the new image tag.
4. Kubernetes reconciles the manifest and rolls out the new version.

That split keeps application delivery and cluster configuration decoupled, which makes the deployment history easier to audit and revert.

## Notes for contributors

- Keep the API endpoints simple and explicit.
- Keep health probes fast and dependency-light.
- Update this README whenever routes, ports, or deployment expectations change.
- If you change the container port, update the Dockerfile and the GitOps service and deployment manifests together.
