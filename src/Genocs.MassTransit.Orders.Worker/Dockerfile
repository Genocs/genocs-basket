#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Genocs.MassTransit.Orders.Contracts/Genocs.MassTransit.Orders.Contracts.csproj", "Genocs.MassTransit.Orders.Contracts/"]
COPY ["Genocs.MassTransit.Orders.Worker/Genocs.MassTransit.Orders.Worker.csproj", "Genocs.MassTransit.Orders.Worker/"]
RUN dotnet restore "Genocs.MassTransit.Orders.Worker/Genocs.MassTransit.Orders.Worker.csproj"
COPY . .
WORKDIR "/src/Genocs.MassTransit.Orders.Worker"
RUN dotnet build "Genocs.MassTransit.Orders.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Genocs.MassTransit.Orders.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.MassTransit.Orders.Worker.dll"]