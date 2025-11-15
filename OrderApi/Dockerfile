# مرحله ۱: Build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# کپی فایل‌های پروژه

COPY OrderApi.csproj .
RUN dotnet restore

# کپی کل کد و build

COPY . .
RUN dotnet publish -c Release -o /app/publish

# مرحله ۲: Runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "OrderApi.dll"]