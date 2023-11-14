#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Gateways/YARP.Proxy/YARP.Proxy.csproj", "src/Gateways/YARP.Proxy/"]
COPY ["src/Shared/Shared.Utilites/Shared.Utilites.csproj", "src/Shared/Shared.Utilites/"]
RUN dotnet restore "src/Gateways/YARP.Proxy/YARP.Proxy.csproj"
COPY . .
WORKDIR "/src/src/Gateways/YARP.Proxy"
RUN dotnet build "YARP.Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YARP.Proxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YARP.Proxy.dll"]