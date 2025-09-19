# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY SanadAPI/SanadAPI.sln .
COPY SanadAPI/*.csproj ./SanadAPI/
RUN dotnet restore
COPY SanadAPI/. ./SanadAPI/
WORKDIR /app/SanadAPI
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "SanadAPI.dll"]
