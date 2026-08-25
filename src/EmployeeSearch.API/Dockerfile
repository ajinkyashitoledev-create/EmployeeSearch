# Build context is the repository root (see docker-compose.yml), because
# the API project references sibling projects outside its own folder.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/EmployeeSearch.API/EmployeeSearch.API.csproj", "src/EmployeeSearch.API/"]
COPY ["src/EmployeeSearch.Application/EmployeeSearch.Application.csproj", "src/EmployeeSearch.Application/"]
COPY ["src/EmployeeSearch.Infrastructure/EmployeeSearch.Infrastructure.csproj", "src/EmployeeSearch.Infrastructure/"]
COPY ["src/EmployeeSearch.Domain/EmployeeSearch.Domain.csproj", "src/EmployeeSearch.Domain/"]
RUN dotnet restore "src/EmployeeSearch.API/EmployeeSearch.API.csproj"

COPY src/ src/
WORKDIR "/src/src/EmployeeSearch.API"
RUN dotnet build "EmployeeSearch.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EmployeeSearch.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeSearch.API.dll"]
