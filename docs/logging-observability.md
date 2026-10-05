# Logging & Observability

NuciSearch uses **NuciLog** for structured logging with operations and log info keys. This document details the logging architecture, operations, log info keys, and diagnostic patterns.

## 1. NuciLog Integration

### Configuration

```csharp
// ServiceCollectionExtensions.cs
NuciLoggerSettings loggingSettings = new();
configuration.Bind(nameof(NuciLoggerSettings), loggingSettings);
services.AddSingleton(loggingSettings);

services.AddSingleton<ILogger, NuciLogger>();
```

### NuciLoggerSettings

| Property | Type | Description |
|----------|------|-------------|
| `logFilePath` | `string` | Path to log output file |
| `isFileOutputEnabled` | `bool` | Whether file logging is enabled |

Settings are loaded from `appsettings.json` under the `NuciLoggerSettings` section.

## 2. Operations

Operations represent discrete units of work that can be traced and measured.

### NuciSearchOperation

```csharp
public sealed class NuciSearchOperation : Operation
{
    public static Operation GetCountryCode
        => new NuciSearchOperation(nameof(GetCountryCode));

    public static Operation Search
        => new NuciSearchOperation(nameof(Search));

    private NuciSearchOperation(string name) : base(name) { }
}
```

| Operation | Purpose | Used By |
|-----------|---------|---------|
| `Search` | Query processing and URL generation | `SearchService.GetSearchUrl` |
| `GetCountryCode` | IP geolocation lookup | `GeolocationService.GetCountryCodeAsync` |

### Operation Lifecycle

Each operation logs three events:

1. **Started** — Operation begins
2. **Success** — Operation completed successfully
3. **Failure** — Operation threw an exception

```csharp
// SearchService.GetSearchUrl
logger.Info(NuciSearchOperation.Search, OperationStatus.Started, logInfos);
try
{
    // ... processing ...
    logger.Info(NuciSearchOperation.Search, OperationStatus.Success, successLogInfos);
    return url;
}
catch (Exception exception)
{
    logger.Error(NuciSearchOperation.Search, OperationStatus.Failure, exception, logInfos);
    throw;
}
```

## 3. Log Info Keys

Log info keys provide structured, queryable context for log entries.

### NuciSearchLogInfoKey

```csharp
public sealed class NuciSearchLogInfoKey : LogInfoKey
{
    public static LogInfoKey IpAddress => new NuciSearchLogInfoKey(nameof(IpAddress));
    public static LogInfoKey Query => new NuciSearchLogInfoKey(nameof(Query));
    public static LogInfoKey SearchType => new NuciSearchLogInfoKey(nameof(SearchType));
    public static LogInfoKey Url => new NuciSearchLogInfoKey(nameof(Url));

    private NuciSearchLogInfoKey(string name) : base(name) { }
}
```

| Key | Type | Description | Operations |
|-----|------|-------------|------------|
| `IpAddress` | `string` | Client IP address | `GetCountryCode`, `Search` |
| `Query` | `string` | Raw user query | `Search` |
| `SearchType` | `string` | Search mode (`auto`, `text`, `images`, etc.) | `Search` |
| `Url` | `string` | Generated target URL | `Search` (success only) |

### Usage in SearchService

```csharp
// Start
IEnumerable<LogInfo> logInfos = [
    new(NuciSearchLogInfoKey.Query, rawQuery),
    new(NuciSearchLogInfoKey.SearchType, searchType)
];
logger.Info(NuciSearchOperation.Search, OperationStatus.Started, logInfos);

// Success
IEnumerable<LogInfo> successLogInfos = [
    new(NuciSearchLogInfoKey.Query, rawQuery),
    new(NuciSearchLogInfoKey.SearchType, searchType),
    new(NuciSearchLogInfoKey.Url, url)
];
logger.Info(NuciSearchOperation.Search, OperationStatus.Success, successLogInfos);

// Failure
logger.Error(NuciSearchOperation.Search, OperationStatus.Failure, exception, logInfos);
```

### Usage in GeolocationService

```csharp
// Failure only (success doesn't log additional info)
logger.Error(
    NuciSearchOperation.GetCountryCode,
    OperationStatus.Failure,
    exception,
    [new(NuciSearchLogInfoKey.IpAddress, ipAddress)]);
```

## 4. Log Output Format

NuciLog produces structured JSON logs. Example entries:

### Search Started

```json
{
  "timestamp": "2024-01-15T10:30:45.123Z",
  "level": "Information",
  "operation": "Search",
  "status": "Started",
  "logInfos": {
    "Query": "emag laptop",
    "SearchType": "auto"
  }
}
```

### Search Success

```json
{
  "timestamp": "2024-01-15T10:30:45.125Z",
  "level": "Information",
  "operation": "Search",
  "status": "Success",
  "logInfos": {
    "Query": "emag laptop",
    "SearchType": "auto",
    "Url": "https://emag.ro/search/laptop"
  }
}
```

### Search Failure

```json
{
  "timestamp": "2024-01-15T10:30:45.130Z",
  "level": "Error",
  "operation": "Search",
  "status": "Failure",
  "exception": {
    "type": "System.ArgumentNullException",
    "message": "Value cannot be null.",
    "stackTrace": "..."
  },
  "logInfos": {
    "Query": "emag laptop",
    "SearchType": "auto"
  }
}
```

### Geolocation Failure

```json
{
  "timestamp": "2024-01-15T10:30:45.200Z",
  "level": "Error",
  "operation": "GetCountryCode",
  "status": "Failure",
  "exception": {
    "type": "System.Net.Http.HttpRequestException",
    "message": "Request failed with status code 429",
    "stackTrace": "..."
  },
  "logInfos": {
    "IpAddress": "203.0.113.42"
  }
}
```

## 5. Correlation and Tracing

### Request Correlation

- Each HTTP request gets a `TraceIdentifier` from `HttpContext.TraceIdentifier`
- In development, `Activity.Current?.Id` provides W3C trace context
- The `Error.razor` page displays the Request ID for user-reported issues

### Operation Correlation

- `Search` operation logs include `Query` and `SearchType` for filtering
- `GetCountryCode` operation logs include `IpAddress` for debugging geolocation issues
- Success logs include the generated `Url` for audit trails

## 6. Log Levels

| Level | Usage |
|-------|-------|
| `Information` | Operation Started, Operation Success |
| `Error` | Operation Failure (with exception) |
| `Warning` | Not currently used |
| `Debug` | Not currently used |

## 7. File Logging

When `NuciLoggerSettings.isFileOutputEnabled = true` and `logFilePath` is set:

- Logs are written to the specified file path
- File rotation and retention are handled by NuciLog
- The log file path is operator-controlled (see PRIVACY.md)

## 8. Diagnostics Queries

### Find all searches for a specific query

```bash
jq 'select(.logInfos.Query == "emag laptop")' logfile.json
```

### Find failed geolocation lookups

```bash
jq 'select(.operation == "GetCountryCode" and .status == "Failure")' logfile.json
```

### Find searches by type

```bash
jq 'select(.logInfos.SearchType == "images")' logfile.json
```

### Get all URLs generated for a query

```bash
jq 'select(.logInfos.Query == "minecraft wiki creeper" and .status == "Success") | .logInfos.Url' logfile.json
```

### Count operations by status

```bash
jq -s 'group_by(.operation) | map({operation: .[0].operation, started: map(select(.status=="Started")) | length, success: map(select(.status=="Success")) | length, failure: map(select(.status=="Failure")) | length})' logfile.json
```

## 9. Adding New Operations

To add a new operation:

1. Add a static property to `NuciSearchOperation`:
   ```csharp
   public static Operation MyNewOperation
       => new NuciSearchOperation(nameof(MyNewOperation));
   ```

2. Add any new `LogInfoKey` to `NuciSearchLogInfoKey` if needed

3. Use in your service:
   ```csharp
   logger.Info(NuciSearchOperation.MyNewOperation, OperationStatus.Started, logInfos);
   try { ... }
   catch (Exception ex) {
       logger.Error(NuciSearchOperation.MyNewOperation, OperationStatus.Failure, ex, logInfos);
       throw;
   }
   ```

## 10. Testing Logging

Tests can verify logging by mocking `ILogger`:

```csharp
Mock<ILogger> loggerMock = new();
SearchService searchService = new SearchService(loggerMock.Object);

searchService.GetSearchUrl("emag laptop", "auto");

loggerMock.Verify(
    l => l.Info(
        NuciSearchOperation.Search,
        OperationStatus.Success,
        It.Is<IEnumerable<LogInfo>>(infos =>
            infos.Any(i => i.Key.Name == "Url" && i.Value.ToString().Contains("emag.ro")))),
    Times.Once);
```