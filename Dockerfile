# Dockerfile for catch-up-platform
# Summary:
# This Dockerfile builds and run the catch-up-platform application using Microsoft .NET SDK 9.0.
# Description:
# This Dockerfile is designed to build an ASP.NET Core application using .NET SDK and run it in a lightweight
# ASP.NET Core 9.0 environment. It uses a multi-stage build to keep the final image size small by separating the build
# and runtime environments. It sets the active profile to 'Production' for production.
# Version: 1.0
# Maintainer: Web Applications Development Team

# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app
COPY CatchUpPlatform.API/*.csproj CatchUpPlatform.API/
RUN dotnet restore ./CatchUpPlatform.API
COPY . .
RUN dotnet publish ./CatchUpPlatform.API -c Release -o out

# Final stage: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "CatchUpPlatform.API.dll"]

# It is necessary to define the following environment variables in the hosting provider for the application to
# run correctly in the Production environment:
# - DATABASE_NAME: The name of the database to connect to.
# - DATABASE_USER: The username for the database connection.
# - DATABASE_PASSWORD: The password for the database connection.
# - DATABASE_URL: The URL of the database to connect to.
# - DATABASE_PORT: The port of the database to connect to.
