FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EZ-ITAM.sln ./
COPY src/EzItam.Web/EzItam.Web.csproj src/EzItam.Web/
COPY tests/EzItam.Tests/EzItam.Tests.csproj tests/EzItam.Tests/
RUN dotnet restore src/EzItam.Web/EzItam.Web.csproj
COPY . .
RUN dotnet publish src/EzItam.Web/EzItam.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    TZ=Asia/Seoul
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EzItam.Web.dll"]
