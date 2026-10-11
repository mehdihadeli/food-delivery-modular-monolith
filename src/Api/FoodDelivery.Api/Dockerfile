# syntax=docker/dockerfile:1
# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md

# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# The API references modules and the Squirrel building blocks, so copy the
# repository source tree before restore.
COPY nuget.config global.json version.json ./
COPY src/ src/

RUN dotnet restore src/Api/FoodDelivery.Api/FoodDelivery.Api.csproj -a $TARGETARCH

# Publish the API. Version stamping is skipped because the Docker build context
# does not contain git metadata.
RUN dotnet publish src/Api/FoodDelivery.Api/FoodDelivery.Api.csproj \
  --configuration Release \
  --no-restore \
  -a $TARGETARCH \
  -p:SkipVersion=true \
  -p:RunAnalyzers=false \
  -o /app

# Runtime stage
# Alpine ASP.NET images ship without ICU or tzdata. Add them and disable
# globalization-invariant mode so culture-aware APIs and time zones work:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  LC_ALL=en_US.UTF-8 \
  LANG=en_US.UTF-8

RUN apk add --no-cache \
  icu-data-full \
  icu-libs \
  tzdata

WORKDIR /app
EXPOSE 8080

# Bind Kestrel to the container port and reuse the environment-specific
# appsettings.docker.json. Override ASPNETCORE_ENVIRONMENT at run time when
# needed.
ENV ASPNETCORE_HTTP_PORTS=8080 \
  ASPNETCORE_ENVIRONMENT=docker

# Copy the published output and run as the image's built-in non-root user.
COPY --link --from=build /app .

USER $APP_UID

ENTRYPOINT ["./FoodDelivery.Api"]
