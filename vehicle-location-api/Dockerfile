FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["vehicle-location-api/vehicle-location-api.csproj", "vehicle-location-api/"]
RUN dotnet restore "vehicle-location-api/vehicle-location-api.csproj"
COPY . .
WORKDIR "/src/vehicle-location-api"
RUN dotnet build "vehicle-location-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vehicle-location-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vehicle-location-api.dll"]
