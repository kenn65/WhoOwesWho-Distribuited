# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5030
EXPOSE 5031


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["WhoOwesWho.UserService/WhoOwesWho.UserService.csproj", "WhoOwesWho.UserService/"]
COPY ["WhoOwesWho.UserService/appsettings.json", "WhoOwesWho.EncryptionService/"]
COPY ["WhoOwesWho.Models/WhoOwesWho.Models.csproj", "WhoOwesWho.Models/"]
RUN dotnet restore "./WhoOwesWho.UserService/WhoOwesWho.UserService.csproj"
COPY . .
WORKDIR "/src/WhoOwesWho.UserService"
RUN dotnet build "./WhoOwesWho.UserService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./WhoOwesWho.UserService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY ./certs/aspnetapp.pfx /https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
ENV ASPNETCORE_URLS=https://+:5031;http://+:5030
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhoOwesWho.UserService.dll"]