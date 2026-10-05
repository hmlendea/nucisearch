# Deployment & Operations

NuciSearch is a self-hosted ASP.NET Core Blazor Server application targeting .NET 10. This document covers deployment, configuration, scaling, and operational considerations.

## 1. System Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| **OS** | Linux, macOS, Windows | Linux (systemd) |
| **Runtime** | .NET 10.0 ASP.NET Core Runtime | .NET 10.0 SDK (for building) |
| **RAM** | 512 MB | 1 GB+ |
| **CPU** | 1 vCPU | 2+ vCPU |
| **Disk** | 100 MB | 500 MB+ (logs) |
| **Network** | Outbound HTTPS (ipwho.is, search engines) | Same |

## 2. Build & Publish

### Build

```bash
dotnet build NuciSearch.slnx --configuration Release
```

### Publish (Self-Contained)

```bash
dotnet publish NuciSearch/NuciSearch.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output ./publish
```

### Publish (Framework-Dependent)

```bash
dotnet publish NuciSearch/NuciSearch.csproj \
    --configuration Release \
    --output ./publish
```

**Note:** Framework-dependent deployment requires .NET 10.0 runtime on the target machine. Self-contained includes the runtime (~70 MB).

## 3. Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "NuciLoggerSettings": {
    "logFilePath": "logs/nucisearch.log",
    "isFileOutputEnabled": false
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "NuciLoggerSettings": {
    "logFilePath": "logs/nucisearch.log",
    "isFileOutputEnabled": true
  }
}
```

### Configuration Keys

| Section | Key | Type | Default | Description |
|---------|-----|------|---------|-------------|
| `Logging:LogLevel:Default` | `Default` | `string` | `Information` | Default log level |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Microsoft.AspNetCore` | `string` | `Warning` | ASP.NET Core log level |
| `AllowedHosts` | `*` | `string` | `*` | Host filtering |
| `NuciLoggerSettings:logFilePath` | `logFilePath` | `string` | `logs/nucisearch.log` | Log file path |
| `NuciLoggerSettings:isFileOutputEnabled` | `isFileOutputEnabled` | `bool` | `false` | Enable file logging |

### Environment Variables

All configuration keys can be overridden via environment variables:

```bash
export Logging__LogLevel__Default=Debug
export NuciLoggerSettings__logFilePath=/var/log/nucisearch.log
export NuciLoggerSettings__isFileOutputEnabled=true
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
```

## 4. Running the Application

### Direct Execution

```bash
# Development
dotnet run --project NuciSearch/NuciSearch.csproj

# Production (published)
./publish/NuciSearch
```

### With Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production \
ASPNETCORE_URLS=http://0.0.0.0:5000 \
NuciLoggerSettings__isFileOutputEnabled=true \
NuciLoggerSettings__logFilePath=/var/log/nucisearch.log \
./publish/NuciSearch
```

## 5. Systemd Service (Linux)

### Service File: `/etc/systemd/system/nucisearch.service`

```ini
[Unit]
Description=NuciSearch - Self-hosted search wrapper
After=network.target

[Service]
Type=notify
ExecStart=/opt/nucisearch/NuciSearch
WorkingDirectory=/opt/nucisearch
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nucisearch
User=nucisearch
Group=nucisearch

# Environment
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=NuciLoggerSettings__isFileOutputEnabled=true
Environment=NuciLoggerSettings__logFilePath=/var/log/nucisearch/nucisearch.log

# Security
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/log/nucisearch
CapabilityBoundingSet=CAP_NET_BIND_SERVICE

[Install]
WantedBy=multi-user.target
```

### Setup Commands

```bash
# Create user
sudo useradd --system --no-create-home --shell /bin/false nucisearch

# Create directories
sudo mkdir -p /opt/nucisearch /var/log/nucisearch
sudo chown nucisearch:nucisearch /opt/nucisearch /var/log/nucisearch

# Copy published files
sudo cp -r ./publish/* /opt/nucisearch/

# Install service
sudo cp nucisearch.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable nucisearch
sudo systemctl start nucisearch

# Check status
sudo systemctl status nucisearch
sudo journalctl -u nucisearch -f
```

## 6. Reverse Proxy (nginx)

### Configuration: `/etc/nginx/sites-available/nucisearch`

```nginx
server {
    listen 80;
    server_name search.nuilandia.ro;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name search.nuilandia.ro;

    ssl_certificate /etc/letsencrypt/live/search.nuilandia.ro/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/search.nuilandia.ro/privkey.pem;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN";
    add_header X-Content-Type-Options "nosniff";
    add_header X-XSS-Protection "1; mode=block";
    add_header Referrer-Policy "strict-origin-when-cross-origin";

    # Proxy to Kestrel
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_send_timeout 300s;
    }

    # Static assets (optional - can be served by Kestrel)
    location /assets/ {
        alias /opt/nucisearch/wwwroot/assets/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    location /styles/ {
        alias /opt/nucisearch/wwwroot/styles/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # OpenSearch
    location = /opensearch.xml {
        proxy_pass http://localhost:5000/opensearch.xml;
        proxy_set_header Host $host;
    }
}
```

### Enable Site

```bash
sudo ln -s /etc/nginx/sites-available/nucisearch /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## 7. HTTPS with Let's Encrypt

```bash
sudo certbot --nginx -d search.nuilandia.ro
```

Certbot will automatically update the nginx configuration with SSL settings.

## 8. Scaling Considerations

### Horizontal Scaling

NuciSearch is **stateless** except for the in-memory geolocation cache. Multiple instances can run behind a load balancer.

```
                    ┌─────────────┐
   Client ─────────►│  Load       │
   (HTTPS)          │  Balancer   │
                    └──────┬──────┘
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
    ┌──────────┐     ┌──────────┐     ┌──────────┐
    │ Instance │     │ Instance │     │ Instance │
    │    A     │     │    B     │     │    C     │
    └──────────┘     └──────────┘     └──────────┘
```

### Session Affinity (Sticky Sessions)

**Required for Blazor Server.** Each Blazor circuit is tied to a specific server instance. The load balancer must route the same client to the same instance for the duration of their session.

| Load Balancer | Sticky Session Configuration |
|---------------|------------------------------|
| nginx | `ip_hash` or `sticky cookie` |
| HAProxy | `balance source` or `cookie` |
| AWS ALB | Target group stickiness (1 hour) |
| Cloudflare | Not supported (use Cloudflare Tunnel + sticky sessions) |

### Geolocation Cache in Clustered Deployment

Each instance has its own `IMemoryCache`. The first request for an IP on any instance hits the ipwho.is API.

**Options for shared cache:**
1. **Accept eventual consistency** (current) — Each instance caches independently. Acceptable for culture detection.
2. **Distributed cache** — Replace `IMemoryCache` with `IDistributedCache` (Redis, SQL Server). Requires code changes.
3. **External geolocation service** — Move geolocation to a shared service/API.

### Resource Scaling

| Metric | Scaling Trigger |
|--------|-----------------|
| CPU > 70% | Add instance |
| Memory > 80% | Add instance |
| Circuit count > 1000/instance | Add instance |
| Response time > 500ms | Add instance |

## 9. Monitoring

### Health Checks

No built-in health check endpoint. Add one if needed:

```csharp
// Program.cs
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

### Logs

- **Structured JSON** via NuciLog (see [Logging & Observability](logging-observability.md))
- **File output** when `NuciLoggerSettings.isFileOutputEnabled = true`
- **Console output** always available via systemd/journalctl

### Key Metrics to Monitor

| Metric | Source | Alert Threshold |
|--------|--------|-----------------|
| Request rate | nginx access logs | N/A |
| Error rate | nginx error logs / NuciLog | > 1% |
| Response time | nginx access logs | > 1s p95 |
| Circuit count | Custom metric | > 1000/instance |
| Geolocation API errors | NuciLog | > 10/min |
| Memory usage | systemd / Prometheus | > 80% |
| CPU usage | systemd / Prometheus | > 70% |

### Log Aggregation

Recommended: Ship logs to Loki, Elasticsearch, or similar.

```bash
# Example: Promtail for Loki
# /etc/promtail/config.yml
clients:
  - url: http://loki:3100/loki/api/v1/push

positions:
  filename: /tmp/positions.yaml

scrape_configs:
  - job_name: nucisearch
    static_configs:
      - targets: [localhost]
        labels:
          job: nucisearch
          __path__: /var/log/nucisearch/*.log
```

## 10. Backup & Recovery

### What to Backup

| Path | Description | Frequency |
|------|-------------|-----------|
| `/opt/nucisearch` | Application binaries | On deploy |
| `/var/log/nucisearch` | Log files | Daily |
| `/etc/nginx/sites-available/nucisearch` | nginx config | On change |
| `/etc/systemd/system/nucisearch.service` | systemd unit | On change |
| `/etc/letsencrypt` | SSL certificates | Let's Encrypt auto-renews |

### Recovery Procedure

1. **Restore binaries** to `/opt/nucisearch`
2. **Restore configs** to `/etc/nginx/`, `/etc/systemd/`
3. **Restart services:**
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl restart nucisearch nginx
   ```

## 11. Updates & Maintenance

### Update Process

```bash
# 1. Pull latest code
git pull origin main

# 2. Build and publish
dotnet publish NuciSearch/NuciSearch.csproj -c Release -o ./publish

# 3. Stop service
sudo systemctl stop nucisearch

# 4. Backup current version
sudo cp -r /opt/nucisearch /opt/nucisearch.backup.$(date +%Y%m%d)

# 5. Deploy new version
sudo cp -r ./publish/* /opt/nucisearch/

# 6. Start service
sudo systemctl start nucisearch

# 7. Verify
sudo systemctl status nucisearch
curl -f https://search.nuilandia.ro/
```

### Rollback

```bash
sudo systemctl stop nucisearch
sudo rm -rf /opt/nucisearch/*
sudo cp -r /opt/nucisearch.backup.20240115/* /opt/nucisearch/
sudo systemctl start nucisearch
```

## 12. Security Hardening

### Application

- Runs as non-root user (`nucisearch`)
- `NoNewPrivileges=true` in systemd
- `PrivateTmp=true`, `ProtectSystem=strict`, `ProtectHome=true`
- Only `CAP_NET_BIND_SERVICE` if binding to port < 1024 (not needed with reverse proxy)

### Network

- Kestrel binds to `localhost:5000` only (not exposed publicly)
- nginx terminates TLS and proxies to localhost
- Firewall: Only ports 80, 443 open externally

### Headers

nginx adds security headers:
- `X-Frame-Options: SAMEORIGIN`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`

### Dependencies

- Regular `dotnet list package --vulnerable --include-transitive`
- Update NuGet packages monthly
- Monitor NuciLog, NuciText.Obfuscation for security advisories

## 13. Troubleshooting

### Common Issues

| Symptom | Cause | Solution |
|---------|-------|----------|
| "Connection refused" | Kestrel not running | `systemctl status nucisearch` |
| 502 Bad Gateway | Kestrel crashed | Check logs: `journalctl -u nucisearch` |
| Culture always en-GB | X-Forwarded-For not passed | Check nginx `proxy_set_header X-Forwarded-For` |
| Geolocation always GB | ipwho.is blocked | Check outbound HTTPS from server |
| High memory | Circuit leak | Check for long-running circuits, restart periodically |
| Slow searches | External API latency | Monitor ipwho.is, search engine response times |

### Debug Commands

```bash
# View logs
sudo journalctl -u nucisearch -f

# View last 100 lines
sudo journalctl -u nucisearch -n 100

# Check process
ps aux | grep NuciSearch

# Check ports
ss -tlnp | grep 5000

# Test geolocation
curl "https://ipwho.is/8.8.8.8?fields=country_code"

# Test app directly
curl -v http://localhost:5000/

# Test OpenSearch
curl https://search.nuilandia.ro/opensearch.xml
```

## 14. Release Process

The project uses `release.sh` for automated releases:

```bash
./release.sh 1.2.3
```

This script:
1. Updates version in project files
2. Creates git tag
3. Builds and publishes
4. Creates GitHub release
5. Uploads artefacts

See `release.sh` for details.