#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FinancialChat.Bot.Api/FinancialChat.Bot.App.csproj", "FinancialChat.Bot.Api/"]
COPY ["FinancialChat.Core/FinancialChat.Core.csproj", "FinancialChat.Core/"]
COPY ["FinancialChat.Bot/FinancialChat.Bot.csproj", "FinancialChat.Bot/"]
RUN dotnet restore "FinancialChat.Bot.Api/FinancialChat.Bot.App.csproj"
COPY . .
WORKDIR "/src/FinancialChat.Bot.Api"
RUN dotnet build "FinancialChat.Bot.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinancialChat.Bot.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialChat.Bot.App.dll"]