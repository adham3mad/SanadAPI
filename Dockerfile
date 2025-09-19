# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file
COPY SanadAPI.sln .

# Copy project files
COPY SanadAPI/*.csproj ./SanadAPI/
RUN dotnet restore

# Copy everything else
COPY SanadAPI/. ./SanadAPI/
WORKDIR /app/SanadAPI
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "SanadAPI.dll"]
