# Use the official .NET 6.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ConstructionTracker.Web.Host/ConstructionTracker.Web.Host.csproj", "src/ConstructionTracker.Web.Host/"]
COPY ["src/ConstructionTracker.Web.Core/ConstructionTracker.Web.Core.csproj", "src/ConstructionTracker.Web.Core/"]
COPY ["src/ConstructionTracker.Application/ConstructionTracker.Application.csproj", "src/ConstructionTracker.Application/"]
COPY ["src/ConstructionTracker.Core/ConstructionTracker.Core.csproj", "src/ConstructionTracker.Core/"]
COPY ["src/ConstructionTracker.EntityFrameworkCore/ConstructionTracker.EntityFrameworkCore.csproj", "src/ConstructionTracker.EntityFrameworkCore/"]

RUN dotnet restore "src/ConstructionTracker.Web.Host/ConstructionTracker.Web.Host.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/src/ConstructionTracker.Web.Host"
RUN dotnet build "ConstructionTracker.Web.Host.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ConstructionTracker.Web.Host.csproj" -c Release -o /app/publish

# Final stage - runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Update the ENTRYPOINT to use the correct DLL name
ENTRYPOINT ["dotnet", "ConstructionTracker.Web.Host.dll"]

