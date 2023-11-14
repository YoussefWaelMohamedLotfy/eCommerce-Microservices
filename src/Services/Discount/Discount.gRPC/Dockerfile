#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Services/Discount/Discount.gRPC/Discount.gRPC.csproj", "src/Services/Discount/Discount.gRPC/"]
COPY ["src/Shared/Shared.Utilites/Shared.Utilites.csproj", "src/Shared/Shared.Utilites/"]
RUN dotnet restore "src/Services/Discount/Discount.gRPC/Discount.gRPC.csproj"
COPY . .
WORKDIR "/src/src/Services/Discount/Discount.gRPC"
RUN dotnet build "Discount.gRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discount.gRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discount.gRPC.dll"]