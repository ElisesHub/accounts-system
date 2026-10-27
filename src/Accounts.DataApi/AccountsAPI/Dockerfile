FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

#  Project file only, at its real path → Nuget packages are cached
COPY ["src/Accounts.DataApi/AccountsAPI/AccountsAPI.csproj", "src/Accounts.DataApi/AccountsAPI/"]
RUN dotnet restore "src/Accounts.DataApi/AccountsAPI/AccountsAPI.csproj"

COPY ["protos/", "protos/"]
COPY ["src/Accounts.DataApi/AccountsAPI/", "src/Accounts.DataApi/AccountsAPI/"]

# Work from the project folder so the build/publish lines stay short
WORKDIR /src/src/Accounts.DataApi/AccountsAPI
RUN dotnet build "AccountsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AccountsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountsAPI.dll"]