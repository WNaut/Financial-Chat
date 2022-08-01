#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_6.x | bash - && \
    apt-get install -y build-essential nodejs
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FinancialChat.WebApp/FinancialChat.WebApp.csproj", "FinancialChat.WebApp/"]
COPY ["FinancialChat.Jwt/FinancialChat.Jwt.csproj", "FinancialChat.Jwt/"]
COPY ["FinancialChat.Core/FinancialChat.Core.csproj", "FinancialChat.Core/"]
COPY ["FinancialChat.Persistence/FinancialChat.Persistence.csproj", "FinancialChat.Persistence/"]
COPY ["FinancialChat.Messaging/FinancialChat.Messaging.csproj", "FinancialChat.Messaging/"]
RUN dotnet restore "FinancialChat.WebApp/FinancialChat.WebApp.csproj"
COPY . .
WORKDIR "/src/FinancialChat.WebApp"
RUN dotnet build "FinancialChat.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinancialChat.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialChat.WebApp.dll"]
