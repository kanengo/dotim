FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Linker/Linker/Linker.csproj", "Linker/Linker/"]
RUN dotnet restore "Linker/Linker/Linker.csproj"
COPY . .
WORKDIR "/src/Linker/Linker"
RUN dotnet build "Linker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Linker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Linker.dll"]
