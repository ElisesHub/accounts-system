FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Accounts.OrchestrationApi/OrchestrationApi/OrchestrationApi.csproj", "src/Accounts.OrchestrationApi/OrchestrationApi/"]

RUN dotnet restore "src/Accounts.OrchestrationApi/OrchestrationApi/OrchestrationApi.csproj"

COPY ["protos/", "protos/"]
COPY ["src/Accounts.OrchestrationApi/OrchestrationApi/", "src/Accounts.OrchestrationApi/OrchestrationApi/"]

# Work from the project folder so the build/publish lines stay short
WORKDIR /src/src/Accounts.OrchestrationApi/OrchestrationApi


RUN dotnet build "OrchestrationApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrchestrationApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src
ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile"]


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrchestrationApi.dll"]