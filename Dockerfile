# 서버 API 이미지
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EZ-ITAM.sln ./
COPY src/EzItam.Contracts/EzItam.Contracts.csproj src/EzItam.Contracts/
COPY src/EzItam.Api/EzItam.Api.csproj src/EzItam.Api/
RUN dotnet restore src/EzItam.Api/EzItam.Api.csproj
COPY . .
RUN dotnet publish src/EzItam.Api/EzItam.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 TZ=Asia/Seoul
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EzItam.Api.dll"]
