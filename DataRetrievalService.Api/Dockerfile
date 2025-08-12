# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY DataRetrievalService.sln ./
COPY DataRetrievalService.Api/DataRetrievalService.Api.csproj DataRetrievalService.Api/
COPY DataRetrievalService.Application/DataRetrievalService.Application.csproj DataRetrievalService.Application/
COPY DataRetrievalService.Infrastructure/DataRetrievalService.Infrastructure.csproj DataRetrievalService.Infrastructure/
COPY DataRetrievalService.Domain/DataRetrievalService.Domain.csproj DataRetrievalService.Domain/
# If you use Central Package Management:
# COPY Directory.Packages.props ./
RUN dotnet restore DataRetrievalService.Api/DataRetrievalService.Api.csproj

COPY . .
WORKDIR /src/DataRetrievalService.Api
RUN dotnet publish DataRetrievalService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DataRetrievalService.Api.dll"]
