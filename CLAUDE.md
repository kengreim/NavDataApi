# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

- Build: `dotnet build`
- Run locally: `dotnet run` (binds to Kestrel defaults; Docker image exposes 8080/8081)
- Restore: `dotnet restore`
- Docker: `docker build -t navdataapi .` then `docker run -p 8080:8080 navdataapi`

There is no test project in the solution.

## Architecture

ASP.NET Core (.NET 9) web API built on **FastEndpoints 8** that serves FAA CIFP-derived SIDs, STARs, approaches, and navigation-point lookups as JSON. The hot data structure is the parsed ARINC 424 database produced by the `arinc424` NuGet package (`Data424`).

### Data lifecycle (CIFP)

The FAA CIFP data is the single source of truth and is loaded into memory at startup, then refreshed in the background:

1. `CifpUpdateService` (Coravel `IInvocable`) is both registered with Coravel's scheduler in `Program.cs:51` (`.Hourly().RunOnceAtStart()`) **and** runs its own `while (!cancelled) { ...; await Task.Delay(1h) }` loop inside `Invoke` (`Services/CifpUpdateService.cs:19`). This is almost certainly a bug — the two hourly loops overlap and the scheduled invocation never returns. Pick one (likely drop the internal delay loop now that Coravel handles cadence) before relying on the update cadence.
2. Each tick GETs `https://external-api.faa.gov/apra/cifp/chart`, deserializes the `ProductSet` XML (`Services/Models/CifpEdition.cs`), and compares the product URL against `_lastKnownUrl`. A new URL triggers a download + `ZipArchive` extraction of the `FAACIFP18` entry.
3. Lines are streamed via `IAsyncEnumerable<string>` into `CifpService.UpdateCifp`, which immediately materializes them with `ToBlockingEnumerable().ToList()` (`Services/CifpService.cs:14`) before calling `Data424.Create(meta, ..., out skipped, out invalid)` with `Supplement.V18`. Don't assume the pipeline actually streams — it doesn't.
4. `Data424.Create` also emits `skipped` (`string[]` of unrecognized line tokens) and `invalid` (`FrozenDictionary<Record424, Diagnostic[]>` of failed builds) that are stored on `CifpService` but currently never read or logged. Either surface them or drop the fields; don't leave them as dead state.
5. `_lastKnownUrl` lives only in the `CifpUpdateService` instance — it resets on every process restart, so a restart re-downloads the current zip even when unchanged.
6. There is **no local-file fallback** on this branch — the committed `FAACIFP18` and csproj `<None Update>` entry were removed, and `CheckForUpdatesAsync` now just logs and rethrows on HTTP failure. Until the first successful fetch, `cifpService.Data` is `null` and any incoming request will throw.

### Service lifetimes (important gotcha)

- `CifpService` — **singleton**, holds the mutable `Data424?`.
- `ArrivalService` / `DepartureService` / `ApproachService` / `PointService` / `AirportService` / `AirwayService` — **scoped**, depend on `CifpService`. All use primary constructors and dereference `cifpService.Data` lazily inside their `Get*` methods, throwing if null.
- Because the local-file fallback was removed, `cifpService.Data` is `null` until the first successful FAA fetch completes. Any request arriving in that window will throw — the services surface it as a clean `Exception("CifpService data is null")` rather than an NPE, but the 500 response is the same either way. If you touch the load flow, preserve the invariant that `Data` is populated before the app starts serving, or add a proper 503 path.

### Endpoints

FastEndpoints 8 auto-discovers endpoint classes under `Endpoints/`. Every route is registered twice — a bare path and a `/v1/` alias — so clients can opt into versioning without a breaking change. Current routes:

- `GET /departures/{AirportId}` (and `/v1/...`) → `GetSidsEndpoint` → `DepartureService.GetCombinedDepartures` → `List<CombinedDeparture>`
- `GET /arrivals/{AirportId}` (and `/v1/...`) → `GetStarsEndpoint` → `ArrivalService.GetCombinedArrivals` → `List<CombinedArrival>`
- `GET /approaches/{AirportId}` (and `/v1/...`) → `GetApproachesEndpoint` → `ApproachService.GetCombinedApproaches` → `List<CombinedApproach>`
- `GET /points/{Identifier}` (and `/v1/...`) → `GetPointEndpoint` → `PointService.GetPointsByIdentifier` → `List<PointLookupResult>`
- `GET /airports/{AirportId}` (and `/v1/...`) → `GetAirportEndpoint` → `AirportService.GetAirportInfo` → `AirportInfo` (returns 404 if not found)
- `GET /airways/{Identifier}` (and `/v1/...`) → `GetAirwaysEndpoint` → `AirwayService.GetCombinedAirways` → `List<CombinedAirway>`

The three procedure endpoints (departures, arrivals, approaches) uppercase the `AirportId` before lookup, and all three services try `Port.Identifier` first then fall back to `Port.Designator`. Keep them in sync — if you add metadata to one, add it to all three; the scaffolding is deliberately identical and diff-reviewable side-by-side. The airport endpoint uses the same dual-lookup pattern but returns a single object or 404 instead of a list. All endpoints return responses via the FastEndpoints 8 `Send.OkAsync(...)` API, not the older `SendAsync`. Each `Configure()` also sets an OpenAPI `Summary(...)` — keep those populated when adding or renaming endpoints because they feed the Scalar UI directly.

The points endpoint searches `EnrouteWaypoints`, `AirportTerminalWaypoints`, `HeliportTerminalWaypoints`, `Omnidirects` (VHF navaids), and `Nondirects` (NDBs), returning **all** matches because ARINC 424 identifiers are not globally unique (e.g. `JFK` is a VOR-DME and a terminal waypoint). Each result carries a `Type` discriminator; terminal waypoints and terminal VORs also carry an `AirportIdentifier`. `Data424.Waypoints` is intentionally skipped — per the arinc424 docs it's a superset of the three typed sub-collections, and iterating it would produce duplicates.

CORS is wide open: `Program.cs:50` calls `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()`. Any future auth, cookie, or hosting work needs to revisit this before it becomes a problem.

**Airport metadata:** `AirportInfo` lives alongside `AirportService` and exposes: `Identifier` (ICAO), `IcaoCode` (two-char region), `Designator` (IATA, nullable), `Name`, `Latitude`/`Longitude`, `Elevation`, `Variation` (magnetic deviation in degrees), `CourseType` (Magnetic/True), `TimeZone`, `Privacy`, `LongestRunwayLength`, `LongestRunwayType`. All enums are serialized as strings via `.ToString()`.

**Airway response shape:** `CombinedAirway` (identifier + points list) and `AirwayFixPoint` live alongside `AirwayService`. Unlike procedure points, airway `Fix` is never null — all fields (`Identifier`, `Latitude`, `Longitude`) are non-nullable. Altitudes use `AltitudeFormatter.FormatAltitude()` directly (no `AltitudeDescription` discriminator). Each point carries `Type` (AirwayType), `Level` (High/Low), `Restriction` (None/Forward/Backward), inbound/outbound courses, `DistanceFrom` (nm), three altitude fields (`Minimum`, `Minimum2`, `Maximum`), and `Descriptions` flags.

Response shape: `CombinedArrival` / `CombinedDeparture` / `CombinedApproach` live alongside their services; the shared `CombinedSequence` (transition name + `TransitionType` stringified from `sequence.Types`) lives in `Services/Models/CombinedSequence.cs` since all three services use it. Each sequence contains `Point`s (`Services/Models/Point.cs`) with nullable `Identifier`/`Latitude`/`Longitude`, altitude min/max, `LegType`, `Course`, and `Descriptions: string[]`. Note `sequence.Types` is plural in `arinc424` 0.3.x — renamed from `Type` during the upgrade — and `.ToString()` on it produces a comma-separated flags string (e.g. `"Runway, AreaNavigation"`), which is a breaking change from the single-value format pre-upgrade.

**Point nullability and leg metadata:** `Point.Fix`-derived fields (`Identifier`, `Latitude`, `Longitude`) are null for path-terminator legs that don't anchor to a waypoint (CA, VA, FM, CD, CI, CR, VD, VI, VM, VR, HA, HF, HM, PI). `Point.LegType` identifies the leg kind, `Point.Course` gives the outbound magnetic course, and `Point.Descriptions` carries ARINC 424 Waypoint Description Code flags (IAF, IF, FAF, MAP, FlyOver, MissedApproachFirstLeg, InitialDeparture, etc.) split into a `string[]`. Clients must handle the null fields; a SID will typically start with a fix-less `CA`/`VA` leg.

**`ProcedurePoint.Descriptions` is `[Obsolete]` in arinc424 0.3.x** with the author's note "maybe split to 4 enums for SID/STAR/Approach and Airway?" — this is the only deprecated symbol in the DLL, and no replacement ships yet. All three procedure services read it; migrate at the next arinc424 upgrade.

### OpenAPI and Scalar UI

`Program.cs` registers `SwaggerDocument` (NSwag via `FastEndpoints.Swagger`) and serves:

- `GET /openapi/v1.json` — generated OpenAPI 3 document (`app.UseOpenApi` with `c.Path = "/openapi/{documentName}.json"`).
- `GET /scalar/v1` — Scalar API reference UI (`Scalar.AspNetCore` 2.13.x; telemetry disabled, Purple/Modern theme, `SchemaPropertyOrder.Preserve` so the JSON shape matches the source DTO order).

`Swagger/TagOrderProcessor.cs` is an NSwag `IDocumentProcessor` that pins configured tag names (currently `"V1"`) to the top of the tag list and sorts the rest alphabetically — register it via `s.DocumentProcessors.Add(new TagOrderProcessor("V1"))` in the `SwaggerDocument` options. If you add a new API version or tag group, update the pinned list here so Scalar renders the groups in a stable order.

### Altitude handling

`AltitudeConverter.ToAltitudeLimitsStrings` maps the ARINC 424 `AltitudeDescription` enum into `(min, max)` strings via `AltitudeFormatter`. All currently-known enum cases are handled (glide/vertical/optional variants were added on this branch for approaches), with a `_ => throw new ArgumentOutOfRangeException(...)` catch-all matching the pattern in `AltitudeFormatter`. A future `arinc424` release that introduces a new enum value will surface as an `ArgumentOutOfRangeException` at runtime — add the case here when you see one.

## External dependencies

- Upstream CIFP source: `https://external-api.faa.gov/apra/cifp/chart` (FAA APRA) returns a `ProductSet` XML whose `edition/product/@url` points at the current CIFP zip.
- `arinc424` (0.3.3) owns `Data424`, `Altitude`, `AltitudeDescription`, and related types referenced throughout `Services/`.
- `FastEndpoints` / `FastEndpoints.Swagger` (8.1.0), `Coravel` (6.0.2), `Scalar.AspNetCore` (2.13.22). Keep these aligned when upgrading — a FastEndpoints major bump in particular changes the `Send*` surface.
