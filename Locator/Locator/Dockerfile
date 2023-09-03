FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Locator/Locator/Locator.csproj", "Locator/Locator/"]
RUN dotnet restore "Locator/Locator/Locator.csproj"
COPY . .
WORKDIR "/src/Locator/Locator"
RUN dotnet build "Locator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Locator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Locator.dll"]
