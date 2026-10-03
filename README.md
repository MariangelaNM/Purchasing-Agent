# Purchasing-Agent

## Installation

### Base de datos de productos

Ejecuta `Database/PurchasingDb.sql` en SQL Server Management Studio o Azure Data Studio. El script crea la base `PurchasingDb`, las tablas `Products` y `Stock` relacionadas por FK, y dos productos de ejemplo.

La API usa por defecto:

```text
Server=localhost;Database=PurchasingDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Si tu servidor es diferente, configura la variable `SQL_CONNECTION_STRING`.

### Docker Desktop

[Download Docker Desktop for Windows](https://docs.docker.com/desktop/setup/install/windows-install/)

### Ollama

[Download Ollama for Windows](https://ollama.com/download/windows)

### Enable virtualization

Run the following commands in **PowerShell**:

```powershell
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
bcdedit /set hypervisorlaunchtype auto
```

Restart your computer, then run:

```powershell
wsl --update
```

### Start Elasticsearch

```powershell
docker run --name rag-elasticsearch -d -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" -e "ES_JAVA_OPTS=-Xms1g -Xmx1g" docker.elastic.co/elasticsearch/elasticsearch:9.4.2
```

Verify that it is running:

```powershell
Invoke-RestMethod http://localhost:9200
```

### Create the index

```http
POST /doc-chunks/_doc
```

```json
{
  "content": "",
  "embedding": "",
  "id": "",
  "ordinal": "",
  "sourceFile": ""
}
```

### Models

```powershell
ollama run nomic-embed-text:latest
```

```powershell
ollama pull qwen2.5:1.5b-instruct
```
