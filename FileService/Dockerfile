FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5290

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_TOKEN
RUN dotnet nuget add source https://nuget.pkg.github.com/qeqig/index.json \
    --name github \
    --username qeqig \
    --password ${GITHUB_TOKEN} \
    --store-password-in-clear-text
COPY ["FileService.Web/FileService.Web.csproj", "FileService.Web/"]
COPY ["FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
COPY ["FileService.Core/FileService.Core.csproj", "FileService.Core/"]
COPY ["FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj", "FileService.Infrastructure.Postgres/"]
COPY ["FileService.Infrastructure.S3/FileService.Infrastructure.S3.csproj", "FileService.Infrastructure.S3/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "FileService.Web/FileService.Web.csproj"
COPY . .
WORKDIR "/src/FileService.Web"
RUN dotnet build "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Web.dll"]
