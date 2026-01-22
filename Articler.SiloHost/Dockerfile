# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Articler.SiloHost/Articler.SiloHost.csproj", "Articler.SiloHost/"]
COPY ["Articler.ServiceDefaults/Articler.ServiceDefaults.csproj", "Articler.ServiceDefaults/"]
COPY ["Articler.AppDomain/Articler.AppDomain.csproj", "Articler.AppDomain/"]
COPY ["Articler.GrainInterfaces/Articler.GrainInterfaces.csproj", "Articler.GrainInterfaces/"]
COPY ["Articler.GrainClasses/Articler.GrainClasses.csproj", "Articler.GrainClasses/"]
RUN dotnet restore "Articler.SiloHost/Articler.SiloHost.csproj"

# Copy source code
COPY . .
WORKDIR "/src/Articler.SiloHost"
RUN dotnet build "Articler.SiloHost.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Articler.SiloHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 11111
EXPOSE 30000

COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Articler.SiloHost.dll"]