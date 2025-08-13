FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/ConstructionTracker.Web.Host/ConstructionTracker.Web.Host.csproj", "src/ConstructionTracker.Web.Host/"]
COPY ["src/ConstructionTracker.Web.Core/ConstructionTracker.Web.Core.csproj", "src/ConstructionTracker.Web.Core/"]
COPY ["src/ConstructionTracker.Application/ConstructionTracker.Application.csproj", "src/ConstructionTracker.Application/"]
COPY ["src/ConstructionTracker.Core/ConstructionTracker.Core.csproj", "src/ConstructionTracker.Core/"]
COPY ["src/ConstructionTracker.EntityFrameworkCore/ConstructionTracker.EntityFrameworkCore.csproj", "src/ConstructionTracker.EntityFrameworkCore/"]
WORKDIR "/src/src/ConstructionTracker.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/ConstructionTracker.Web.Host", "src/ConstructionTracker.Web.Host"]
COPY ["src/ConstructionTracker.Web.Core", "src/ConstructionTracker.Web.Core"]
COPY ["src/ConstructionTracker.Application", "src/ConstructionTracker.Application"]
COPY ["src/ConstructionTracker.Core", "src/ConstructionTracker.Core"]
COPY ["src/ConstructionTracker.EntityFrameworkCore", "src/ConstructionTracker.EntityFrameworkCore"]
WORKDIR "/src/src/ConstructionTracker.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "ConstructionTracker.Web.Host.dll"]
