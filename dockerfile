# Stage 1: Build using .NET 9.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /app

COPY ResearchScraper/ResearchScraper.csproj ./ResearchScraper/
RUN dotnet restore ./ResearchScraper/ResearchScraper.csproj

COPY ResearchScraper/. ./ResearchScraper/
RUN dotnet publish ./ResearchScraper/ResearchScraper.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ResearchScraper.dll"]
