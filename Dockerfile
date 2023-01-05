FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["src/Rn.Timerr/Rn.Timerr.csproj", "Rn.Timerr/"]

RUN dotnet restore "Rn.Timerr/Rn.Timerr.csproj"

COPY ["src/Rn.Timerr/", "Rn.Timerr/"]

WORKDIR "/src/Rn.Timerr"

RUN dotnet build "Rn.Timerr.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rn.Timerr.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rn.Timerr.dll"]
