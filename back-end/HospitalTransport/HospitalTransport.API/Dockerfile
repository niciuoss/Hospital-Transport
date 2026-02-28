FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libgdiplus \
    fontconfig \
    fonts-liberation \
    fonts-dejavu-core \
    wget \
    cabextract \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /usr/share/fonts/truetype/msttcorefonts && \
    cd /tmp && \
    wget -q https://downloads.sourceforge.net/corefonts/arial32.exe && \
    cabextract -d /usr/share/fonts/truetype/msttcorefonts arial32.exe && \
    wget -q https://downloads.sourceforge.net/corefonts/times32.exe && \
    cabextract -d /usr/share/fonts/truetype/msttcorefonts times32.exe && \
    wget -q https://downloads.sourceforge.net/corefonts/verdan32.exe && \
    cabextract -d /usr/share/fonts/truetype/msttcorefonts verdan32.exe && \
    fc-cache -f -v && \
    rm -rf /tmp/*

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