# --- Base Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER root 
# 2. Zainstaluj wymagane biblioteki
RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    gss-ntlmssp \
    krb5-user \
    libkrb5-dev \
    iputils-ping \
    && rm -rf /var/lib/apt/lists/*

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Instalacja Node.js
RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_22.x | bash -
RUN apt-get install -y nodejs

WORKDIR /src

# Kopiujemy pliki projektów i package.json
COPY ["IgniteDigitalization.Server/IgniteDigitalization.Server.csproj", "IgniteDigitalization.Server/"]
COPY ["ignitedigitalization.client/ignitedigitalization.client.esproj", "ignitedigitalization.client/"]
COPY ["ignitedigitalization.client/package.json", "ignitedigitalization.client/package-lock.json", "ignitedigitalization.client/"]


RUN dotnet nuget add source /src --name docker-local-packages

# Przywracamy zależności .NET
RUN dotnet restore "IgniteDigitalization.Server/IgniteDigitalization.Server.csproj"

WORKDIR /src/ignitedigitalization.client
RUN npm ci
RUN npm install --save-dev @rollup/rollup-linux-x64-gnu

# Kopiujemy resztę kodu źródłowego
WORKDIR /src
COPY . .

# --- Publish Stage ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/IgniteDigitalization.Server
RUN dotnet publish "IgniteDigitalization.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# --- Final Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IgniteDigitalization.Server.dll"]
