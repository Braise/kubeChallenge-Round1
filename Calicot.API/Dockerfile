FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 443
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app
COPY Calicot.API/*.csproj /app/Calicot.API/
COPY Calicot.Shared/*csproj /app/Calicot.Shared/

RUN dotnet restore /app/Calicot.API/Calicot.API.csproj

COPY . .
WORKDIR /app/Calicot.API

RUN dotnet build Calicot.API.csproj -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Calicot.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Calicot.API.dll"]