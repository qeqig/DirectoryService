FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5219


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_TOKEN
RUN dotnet nuget add source https://nuget.pkg.github.com/qeqig/index.json \
    --name github \
    --username qeqig \
    --password ${GITHUB_TOKEN} \
    --store-password-in-clear-text
COPY ["src/DirectoryService.Presentation/DirectoryService.Presentation.csproj", "src/DirectoryService.Presentation/"]
COPY ["src/DirectoryService.Application/DirectoryService.Application.csproj", "src/DirectoryService.Application/"]
COPY ["src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "src/DirectoryService.Contracts/"]
COPY ["src/DirectoryService.Domain/DirectoryService.Domain.csproj", "src/DirectoryService.Domain/"]
COPY ["src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "src/DirectoryService.Infrastructure/"]
COPY ["src/DirectoryService.Presenters/DirectoryService.Presenters.csproj", "src/DirectoryService.Presenters/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "src/DirectoryService.Presentation/DirectoryService.Presentation.csproj"
COPY . .
WORKDIR "/src/src/DirectoryService.Presentation"
RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]
