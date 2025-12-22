FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Polls.sln", "./"]
COPY ["Polls.Common/Polls.Common.csproj", "Polls.Common/"]
COPY ["Polls.Infrastructure/Polls.Infrastructure.csproj", "Polls.Infrastructure/"]
COPY ["Polls.REST/Polls.REST.csproj", "Polls.REST/"]

RUN dotnet restore "Polls.REST/Polls.REST.csproj"

COPY . .

WORKDIR "/src/Polls.REST"
RUN dotnet publish "Polls.REST.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Polls.REST.dll"]