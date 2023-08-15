FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Comet/Client.Tests/Client.Tests.csproj", "Comet/Client.Tests/"]
RUN dotnet restore "Comet/Client.Tests/Client.Tests.csproj"
COPY . .
WORKDIR "/src/Comet/Client.Tests"
RUN dotnet build "Client.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Client.Tests.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Client.Tests.dll"]
