FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AttGoraLista.Api/AttGoraLista.Api.csproj AttGoraLista.Api/
RUN dotnet restore AttGoraLista.Api/AttGoraLista.Api.csproj
COPY . .
RUN dotnet publish AttGoraLista.Api/AttGoraLista.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AttGoraLista.Api.dll"]
