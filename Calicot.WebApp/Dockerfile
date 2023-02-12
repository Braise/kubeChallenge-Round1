FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 443
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.SpaProxy

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app
COPY Calicot.WebApp /app/Calicot.WebApp/
COPY Calicot.Shared/*csproj /app/Calicot.Shared/

RUN dotnet restore /app/Calicot.WebApp/Calicot.WebApp.csproj

COPY . .
WORKDIR /app/Calicot.WebApp

RUN dotnet build Calicot.WebApp.csproj -c Release -o /app/build --no-restore
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y build-essential nodejs

FROM build AS publish
RUN dotnet publish "Calicot.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER 1001
ENTRYPOINT ["dotnet", "Calicot.WebApp.dll"]