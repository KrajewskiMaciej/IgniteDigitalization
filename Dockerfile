# =============================================================================
# Etap 1: Budowanie frontendu (Vue + Vite)
# =============================================================================
FROM node:22-alpine AS frontend-build

WORKDIR /app/client

# Kopiuj pliki zależności i pobierz paczki
COPY ignitedigitalization.client/package.json ignitedigitalization.client/package-lock.json ./
RUN npm ci

# Kopiuj resztę kodu frontendu i zbuduj
COPY ignitedigitalization.client/ ./
RUN npm run build-only

# =============================================================================
# Etap 2: Budowanie backendu (.NET 8)
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS backend-build

WORKDIR /app/server

# Kopiuj plik projektu i przywróć zależności (cache-friendly)
COPY IgniteDigitalization.Server/IgniteDigitalization.Server.csproj ./
RUN dotnet restore

# Kopiuj resztę kodu i opublikuj w trybie Release
COPY IgniteDigitalization.Server/ ./
RUN dotnet publish IgniteDigitalization.Server.csproj -c Release -o /publish --no-restore /p:SkipFrontendBuild=true

# Skopiuj zbudowany frontend do wwwroot publikacji
COPY --from=frontend-build /app/client/dist /publish/wwwroot

# =============================================================================
# Etap 3: Obraz produkcyjny (runtime only – bez SDK)
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Zainstaluj libicu (wymagane przez .NET globalizację) i icu-data-full (dla pełnej obsługi locale)
RUN apk add --no-cache icu-libs

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

# Skopiuj opublikowaną aplikację z etapu 2
COPY --from=backend-build /publish ./

# Domyślny port ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Włącz serwowanie plików statycznych frontendu
ENV FrontendSettings__ServeStaticFiles=true

ENTRYPOINT ["dotnet", "IgniteDigitalization.Server.dll"]
