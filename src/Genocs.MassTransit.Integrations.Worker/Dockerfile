#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Genocs.MassTransit.Integrations.Worker/Genocs.MassTransit.Integrations.Worker.csproj", "Genocs.MassTransit.Integrations.Worker/"]
RUN dotnet restore "Genocs.MassTransit.Integrations.Worker/Genocs.MassTransit.Integrations.Worker.csproj"
COPY . .
WORKDIR "/src/Genocs.MassTransit.Integrations.Worker"
RUN dotnet build "Genocs.MassTransit.Integrations.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Genocs.MassTransit.Integrations.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.MassTransit.Integrations.Worker.dll"]