FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY KarateSchool.sln ./
COPY src/KarateSchool.Web/KarateSchool.Web.csproj src/KarateSchool.Web/
COPY tests/KarateSchool.Tests/KarateSchool.Tests.csproj tests/KarateSchool.Tests/
RUN dotnet restore src/KarateSchool.Web/KarateSchool.Web.csproj

COPY . .
RUN dotnet publish src/KarateSchool.Web/KarateSchool.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "KarateSchool.Web.dll"]
