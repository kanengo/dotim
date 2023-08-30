FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ImLogic/ImLogic/ImLogic.csproj", "ImLogic/ImLogic/"]
RUN dotnet restore "ImLogic/ImLogic/ImLogic.csproj"
COPY . .
WORKDIR "/src/ImLogic/ImLogic"
RUN dotnet build "ImLogic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImLogic.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImLogic.dll"]
