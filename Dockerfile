FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY ImageRestApi/ImageRestApi.csproj ./ImageRestApi/ImageRestApi.csproj
COPY ImageRestApi.Tests/ImageRestApi.Tests.csproj ./ImageRestApi.Tests/ImageRestApi.Tests.csproj
RUN dotnet restore

# copy everything else and build app
COPY ImageRestApi/. ./ImageRestApi
COPY ImageRestApi.Tests/. ./ImageRestApi.Tests
WORKDIR /app/ImageRestApi
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/ImageRestApi/out ./
ENTRYPOINT ["dotnet", "ImageRestApi.dll"]
