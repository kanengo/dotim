FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Comet/Comet/Comet.csproj", "Comet/Comet/"]
RUN dotnet restore "Comet/Comet/Comet.csproj"
COPY . .
WORKDIR "/src/Comet/Comet"
RUN dotnet build "Comet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Comet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Comet.dll"]
