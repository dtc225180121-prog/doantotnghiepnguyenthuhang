# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

# Ensure the app listens on the port Render provides via $PORT (fallback to 8080)
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=\"http://+:${PORT:-8080}\"; export ASPNETCORE_URLS; dotnet aoe.dll"]