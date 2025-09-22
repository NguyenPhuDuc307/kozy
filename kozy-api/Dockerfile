FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["kozy-api.csproj", "./"]
RUN dotnet restore "kozy-api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "kozy-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "kozy-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "kozy-api.dll"]
