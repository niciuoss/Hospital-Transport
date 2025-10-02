FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HospitalTransport.API/HospitalTransport.API.csproj", "HospitalTransport.API/"]
COPY ["HospitalTransport.Application/HospitalTransport.Application.csproj", "HospitalTransport.Application/"]
COPY ["HospitalTransport.Domain/HospitalTransport.Domain.csproj", "HospitalTransport.Domain/"]
COPY ["HospitalTransport.Infrastructure/HospitalTransport.Infrastructure.csproj", "HospitalTransport.Infrastructure/"]
RUN dotnet restore "HospitalTransport.API/HospitalTransport.API.csproj"
COPY . .
WORKDIR "/src/HospitalTransport.API"
RUN dotnet build "HospitalTransport.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HospitalTransport.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalTransport.API.dll"]