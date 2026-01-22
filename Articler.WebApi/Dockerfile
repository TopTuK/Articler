# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Articler.WebApi/Articler.WebApi.csproj",						"Articler.WebApi/"]
COPY ["Articler.ServiceDefaults/Articler.ServiceDefaults.csproj",	"Articler.ServiceDefaults/"]
COPY ["Articler.AppDomain/Articler.AppDomain.csproj",				"Articler.AppDomain/"]
COPY ["Articler.GrainInterfaces/Articler.GrainInterfaces.csproj",	"Articler.GrainInterfaces/"]
COPY ["Articler.GrainClasses/Articler.GrainClasses.csproj",			"Articler.GrainClasses/"]
RUN dotnet restore "Articler.WebApi/Articler.WebApi.csproj"

# Copy source code
COPY . .
WORKDIR "/src/Articler.WebApi"
RUN dotnet build "Articler.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Articler.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Articler.WebApi.dll"]