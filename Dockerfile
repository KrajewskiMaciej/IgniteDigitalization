# --- Base Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root 
RUN apt-get update && apt-get install -y --no-install-recommends \
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

# Optymalizacja instalacji Node.js
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && curl -sL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Kopiowanie tylko plików projektowych (do przywrócenia zależności)
COPY ["IgniteDigitalization.Server/IgniteDigitalization.Server.csproj", "IgniteDigitalization.Server/"]
COPY ["ignitedigitalization.client/ignitedigitalization.client.esproj", "ignitedigitalization.client/"]
COPY ["ignitedigitalization.client/package.json", "ignitedigitalization.client/package-lock.json", "ignitedigitalization.client/"]

RUN dotnet nuget add source /src --name docker-local-packages

# Przywracanie zależności .NET z użyciem BuildKit Cache
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "IgniteDigitalization.Server/IgniteDigitalization.Server.csproj"

WORKDIR /src/ignitedigitalization.client

# Przywracanie zależności npm z użyciem BuildKit Cache
RUN --mount=type=cache,id=npm,target=/root/.npm \
    npm ci

# Uwaga: Najlepiej przenieść ten pakiet na stałe do package.json, aby uniknąć ponownego installu.
RUN npm install --save-dev @rollup/rollup-linux-x64-gnu

# Dopiero teraz kopiujemy całą resztę kodu
WORKDIR /src
COPY . .

# --- Publish Stage ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/IgniteDigitalization.Server
# Publish również korzysta z Cache dla NuGet
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "IgniteDigitalization.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# --- Final Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IgniteDigitalization.Server.dll"]