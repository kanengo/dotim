FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Sequence/Sequence/Sequence.csproj", "Sequence/Sequence/"]
RUN dotnet restore "Sequence/Sequence/Sequence.csproj"
COPY . .
WORKDIR "/src/Sequence/Sequence"
RUN dotnet build "Sequence.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sequence.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sequence.dll"]
