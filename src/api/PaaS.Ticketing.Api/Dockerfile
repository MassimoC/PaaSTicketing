FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["api/PaaS.Ticketing.Api/PaaS.Ticketing.Api.csproj", "api/PaaS.Ticketing.Api/"]
COPY ["api/PaaS.Ticketing.ApiLib/PaaS.Ticketing.ApiLib.csproj", "api/PaaS.Ticketing.ApiLib/"]
COPY ["events/PaaS.Ticketing.Events/PaaS.Ticketing.Events.csproj", "events/PaaS.Ticketing.Events/"]
COPY ["security/PaaS.Ticketing.Security/PaaS.Ticketing.Security.csproj", "security/PaaS.Ticketing.Security/"]

RUN dotnet restore "api/PaaS.Ticketing.Api/PaaS.Ticketing.Api.csproj"
COPY . .
WORKDIR "/src/api/PaaS.Ticketing.Api"
RUN dotnet build "PaaS.Ticketing.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PaaS.Ticketing.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PaaS.Ticketing.Api.dll"]