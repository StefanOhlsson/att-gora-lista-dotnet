# Att göra-lista (.NET version)

En fristående .NET/C#-version av fullstackprojektet **Att göra-lista**. Repositoryt är avsett att ligga bredvid originalet och ändrar inte Java/Spring Boot-versionen.

> Rekommenderat GitHub-namn: `att-gora-lista-dotnet`  
> Visningsnamn i dokumentation: **att-gora-lista (.NET version)**

## Syfte

Projektet visar hur samma funktionalitet kan byggas med:

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core och PostgreSQL
- **Frontend:** React, TypeScript, Vite och Axios
- **API-dokumentation:** Swagger/OpenAPI
- **Tester:** xUnit och EF Core InMemory
- **Drift:** Docker och Docker Compose

API-kontraktet följer originalets huvudendpoints så att frontend kan använda backend utan att känna till om implementationen är Java eller C#:

| Metod | Endpoint | Beskrivning |
|---|---|---|
| GET | `/api/todos` | Hämta alla uppgifter |
| GET | `/api/todos/{id}` | Hämta en uppgift |
| POST | `/api/todos` | Skapa uppgift |
| PUT | `/api/todos/{id}` | Uppdatera uppgift |
| PATCH | `/api/todos/{id}/status` | Ändra status och skapa historik |
| DELETE | `/api/todos/{id}` | Ta bort uppgift |

## Arkitektur

```text
frontend (React, lokalt 5173 / Docker http://localhost:5174)
        |
        | HTTP / JSON
        v
backend (ASP.NET Core, lokalt 8080 / Docker 8081)
        |
        | Entity Framework Core
        v
PostgreSQL (lokalt 5432 / Docker 5433)
```

Backend är uppdelad i tydliga lager:

- `Controllers`: HTTP, statuskoder och validering av anrop
- `Services`: affärslogik och statusändringar
- `Data`: Entity Framework Core och exempeldata
- `Models`: databasentiteter och enum-värden
- `Dtos`: API-kontrakt som skyddar databasen från direkt exponering
- `Mapping`: konvertering mellan entiteter och DTO:er

## Förutsättningar

### Alternativ A: Docker

- Docker Desktop
- Docker Compose v2

### Alternativ B: Lokal utveckling

- .NET 8 SDK
- Node.js 20 eller senare
- PostgreSQL 16
- Valfri editor, exempelvis Visual Studio Community 2022, Rider eller VS Code

## Starta hela lösningen med Docker

Från repositoryts rot:

```bash
docker compose up --build
```

Första gången tar bygget en stund eftersom Docker-avbildningarna för backend och frontend byggs. Applikationen är uppe när loggen visar att backend lyssnar (`Now listening on: http://[::]:8080`) och frontend-containern har startat.

### Använd applikationen

När applikationen är uppe startar man en webbläsare och går till:

**http://localhost:5174/**

Där visas att göra-listan, och du kan skapa, redigera, ändra status på och ta bort uppgifter. Exempeldata skapas automatiskt första gången databasen startas.

Övriga adresser när lösningen körs i Docker:

| Tjänst | Adress |
|---|---|
| Frontend (webbapplikationen) | http://localhost:5174/ |
| Backend API | http://localhost:8081/api/todos |
| Swagger | http://localhost:8081/swagger |
| PostgreSQL | `localhost:5433` (användare `postgres`, lösenord `postgres`) |

> Portarna på värddatorn (5174, 8081 och 5433) skiljer sig från standardportarna så att Docker-versionen kan köras samtidigt som en lokal utvecklingsmiljö eller originalprojektet.

Stoppa lösningen:

```bash
docker compose down
```

Stoppa och radera även databasvolymen:

```bash
docker compose down -v
```

## Starta lokalt utan Docker

### 1. Starta PostgreSQL

Skapa en databas och användare, eller använd PostgreSQL från `docker-compose.yml`:

```bash
docker compose up db -d
```

Observera att databasen från `docker-compose.yml` exponeras på port **5433** på värddatorn. Använd i så fall `Port=5433` i anslutningssträngen (se steg 2).

### 2. Starta backend

```bash
cd backend/AttGoraLista.Api
dotnet restore
dotnet run
```

Backend använder anslutningen i `appsettings.Development.json`. Miljövariabeln nedan kan användas för att skriva över den:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=attgoralista;Username=postgres;Password=postgres"
dotnet run
```

### 3. Starta frontend

Öppna en ny terminal:

```bash
cd frontend
npm install
npm run dev
```

Öppna sedan http://localhost:5173/ i webbläsaren. Backend-API:t nås på http://localhost:8080/api/todos.

## API-exempel

### Skapa en uppgift

```bash
curl -X POST http://localhost:8080/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Förbered demo","description":"Skapa wow-effekt","priority":"KRITISK","status":"REGISTRERAD"}'
```

### Ändra status

```bash
curl -X PATCH http://localhost:8080/api/todos/1/status \
  -H "Content-Type: application/json" \
  -d '{"status":"STARTAD"}'
```

Tillåtna statusvärden:

- `REGISTRERAD`
- `STARTAD`
- `AVSLUTAD`

Tillåtna prioritetsvärden:

- `LAG`
- `NORMAL`
- `HOG`
- `KRITISK`

## Databas och migreringar

För enkel start använder projektet `Database.EnsureCreated()`. Det gör att databasen och tabellerna skapas automatiskt i demo- och utbildningsmiljö.

För produktion bör du ersätta detta med EF Core-migreringar:

```bash
cd backend/AttGoraLista.Api
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Byt därefter `EnsureCreatedAsync()` mot `MigrateAsync()` i `Program.cs`.

## Tester

Kör backendtesterna från repositoryts rot:

```bash
dotnet test backend/AttGoraLista.sln
```

Testerna verifierar bland annat att:

- en uppgift kan skapas
- statusändring skapar en historikpost
- en saknad uppgift ger ett kontrollerat resultat

## Konfiguration

Viktiga miljövariabler:

| Variabel | Exempel | Funktion |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=db;...` | PostgreSQL-anslutning |
| `ASPNETCORE_ENVIRONMENT` | `Development` | .NET-miljö |
| `VITE_API_BASE_URL` | `http://localhost:8080/api` | Frontendens API-basadress |

## Felhantering

- Ogiltiga DTO:er ger HTTP 400.
- Okända ID:n ger HTTP 404.
- Oväntade fel fångas av ASP.NET Cores problem-details-hantering.
- Enum-värden serialiseras som text för att göra JSON lättläst.

## CORS

I utvecklingsläge tillåts frontend på `http://localhost:5173`, och i Docker på `http://localhost:5174`. För produktion ska tillåtna origins begränsas till den verkliga domänen.

## Skapa ett nytt GitHub-repository

GitHub tillåter normalt inte parenteser och blanksteg i repositorynamn. Använd därför `att-gora-lista-dotnet` och behåll **Att göra-lista (.NET version)** som projekttitel.

```bash
git init
git add .
git commit -m "Initial .NET version"
git branch -M main
git remote add origin https://github.com/StefanOhlsson/att-gora-lista-dotnet.git
git push -u origin main
```

Skapa repositoryt på GitHub innan sista två kommandona, eller använd GitHub CLI:

```bash
gh repo create StefanOhlsson/att-gora-lista-dotnet --public --source=. --remote=origin --push
```

## Fortsatt utveckling

Lämpliga nästa steg:

1. Lägg till autentisering med OpenID Connect och JWT.
2. Ersätt `EnsureCreated` med riktiga migreringar.
3. Lägg till integrationstester med Testcontainers för PostgreSQL.
4. Lägg till GitHub Actions för build och test.
5. Lägg till pagination, filtrering och optimistic concurrency.

## Licens

Lägg till önskad licens innan repositoryt publiceras. Ingen licensfil genereras automatiskt eftersom licensvalet är ett juridiskt beslut för repositoryägaren.
