# CoffeeNChill - Cloud Menu & Document Management System

## 1. Project Description

CoffeeNChill is a cloud-based menu and document management system designed for the CoffeeNChill coffee shop chain. The system modernizes operations by replacing paper-based menus and file cabinets with a scalable, cloud-native solution.

### Key Features:
- **Digital Menu Management**: Dynamically manage menu items with real-time price and availability updates
- **Staff Document Management**: Centralized storage for operational documents (recipes, manuals, policies)
- **REST API Endpoints**: Comprehensive HTTP API for menu and document operations
- **Azure Cloud Native**: Built on Azure Functions, Table Storage, and File Shares
- **Docker Support**: Full containerization for local development and deployment

## 2. Technologies Used

- **Language**: C# (.NET 10.0)
- **Runtime**: Azure Functions v4 (Isolated Worker Model)
- **Cloud Services**:
  - Azure Table Storage (menu items database)
  - Azure File Share (staff documents storage)
  - Azure Application Insights (logging & monitoring)
- **Local Development**:
  - Azurite (Azure Storage Emulator)
  - Docker & Docker Compose
- **Testing**: Postman
- **Version Control**: Git

## 3. Prerequisites

### System Requirements
- Windows, macOS, or Linux
- Docker & Docker Compose installed
- .NET 10.0 SDK (for local development)
- Postman (for API testing)
- Git

### Required NuGet Packages
```
Azure.Data.Tables (v12.12.0)
Azure.Storage.Files.Shares (v12.27.1)
Microsoft.ApplicationInsights.WorkerService (v3.1.2)
Microsoft.Azure.Functions.Worker (v2.52.0)
Microsoft.Azure.Functions.Worker.ApplicationInsights (v2.51.0)
Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore (v2.1.1)
Microsoft.Azure.Functions.Worker.Sdk (v2.1.0)
```

## 4. Setup Instructions

### Local Development Setup

#### Step 1: Clone Repository
```bash
git clone https://github.com/ST10350498/CLDV-CoffeeNChill-CM-System.git
cd CLDV-CoffeeNChill-CM-System
```

#### Step 2: Restore Dependencies
```bash
dotnet restore CoffeeNChill-cloud.csproj
```

#### Step 3: Start Azurite Storage Emulator

**Option A: Using Docker Compose (Recommended)**
```bash
docker-compose up -d azurite
```

**Option B: Using Docker Standalone**
```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite:3.20.0
```

**Option C: Using provided script**
```bash
chmod +x scripts/start-azurite.sh
./scripts/start-azurite.sh
```

#### Step 4: Run Azure Functions Locally
```bash
func start
```

Functions will be available at: `http://localhost:7071`

### Standalone Docker Execution

#### Build the Docker Image
```bash
docker build -f Dockerfile -t coffeenchill-functions:v1.0 .
```

#### Run with Docker Compose
```bash
docker-compose up --build
```

This starts both Azurite (port 10002) and Azure Functions (port 7071).

#### Run Functions Container Only
```bash
docker run -p 7071:80 \
  -e AzureWebJobsStorage="UseDevelopmentStorage=true" \
  coffeenchill-functions:v1.0
```

## 5. API Endpoints

### Base URL
- **Local**: `http://localhost:7071/api`
- **Production**: `https://<your-function-app>.azurewebsites.net/api`

### Menu Management

#### POST /api/menu
**Create a new menu item**

Request Body:
```json
{
  "category": "Hot Drinks",
  "sku": "COF-001",
  "name": "Espresso",
  "description": "Strong single shot of espresso",
  "price": 2.50,
  "isAvailable": true
}
```

Response: `201 Created` with created item

#### GET /api/menu
**Retrieve all menu items**

Response: Array of all menu items

#### GET /api/menu/category/{category}
**Filter menu items by category**

Example: `GET /api/menu/category/Hot%20Drinks`

Response: Array of items in category

#### PUT /api/menu/{category}/{id}
**Update a menu item**

Example: `PUT /api/menu/Hot%20Drinks/COF-001`

Request Body:
```json
{
  "category": "Hot Drinks",
  "sku": "COF-001",
  "name": "Espresso",
  "description": "Strong single shot",
  "price": 3.00,
  "isAvailable": true
}
```

Response: `200 OK` with updated item

#### DELETE /api/menu/{category}/{id}
**Delete a menu item**

Example: `DELETE /api/menu/Hot%20Drinks/COF-001`

Response: `204 No Content`

### Document Management

#### POST /api/documents/upload
**Upload a staff document**

- Content-Type: `multipart/form-data`
- Field: `file` (required)
- Allowed types: .pdf, .doc, .docx, .txt

Response: `200 OK` with file details

#### GET /api/documents
**List all documents**

Response:
```json
[
  {
    "fileName": "barista-training.pdf",
    "size": 2048000,
    "lastModified": "2026-09-15T09:00:00Z",
    "contentType": "application/pdf"
  }
]
```

#### GET /api/documents/download/{fileName}
**Download a document**

Example: `GET /api/documents/download/barista-training.pdf`

Response: File content with appropriate Content-Type header

## 6. Testing

### Using Postman
1. Open Postman
2. Import: `docs/postman/CoffeeNChill_Part1_Collection.json`
3. All endpoints are pre-configured
4. Test the following workflow:
   - POST /api/menu → Create item
   - GET /api/menu → List all items
   - GET /api/menu/category/{category} → Filter by category
   - PUT /api/menu/{category}/{id} → Update item
   - POST /api/documents/upload → Upload document
   - GET /api/documents → List documents
   - GET /api/documents/download/{fileName} → Download document
   - DELETE /api/menu/{category}/{id} → Delete item

### Using curl

```bash
# Create menu item
curl -X POST http://localhost:7071/api/menu \
  -H "Content-Type: application/json" \
  -d '{
    "category": "Hot Drinks",
    "sku": "COF-001",
    "name": "Espresso",
    "description": "Strong single shot",
    "price": 2.50,
    "isAvailable": true
  }'

# Get all items
curl http://localhost:7071/api/menu

# Upload document
curl -X POST http://localhost:7071/api/documents/upload \
  -F "file=@path/to/document.pdf"
```

## 7. Architecture

### Component Interaction

```
Client Apps (Web, Mobile, Postman)
         ↓
Azure Functions (HTTP Triggers)
   - CreateMenuItem
   - GetAllMenuItems
   - GetMenuItemsByCategory
   - UpdateMenuItem
   - DeleteMenuItem
   - UploadStaffDocument
   - ListStaffDocuments
   - DownloadStaffDocument
         ↓
    ┌────┴────┐
    ↓         ↓
Table Storage  File Share
(MenuItems)    (staff-docs)
```

### Data Model

**MenuItem (Table Storage)**
- PartitionKey: Category (e.g., "Hot Drinks")
- RowKey: SKU/ID (e.g., "COF-001")
- Name: Item name
- Description: Menu description
- Price: Item price (double)
- IsAvailable: Availability status (bool)
- Timestamp: Last modified (auto)
- ETag: Optimistic concurrency (auto)

**DocumentInfo (File Share Metadata)**
- FileName: Name of file
- Size: File size in bytes
- LastModified: Modification timestamp
- ContentType: MIME type

## 8. Docker Setup

### Docker Compose Configuration

```yaml
version: '3.8'
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite:3.20.0
    container_name: coffeenchill-azurite
    ports:
      - "10000:10000" # Blob
      - "10001:10001" # Queue
      - "10002:10002" # File
    volumes:
      - ./data/azurite:/data

  functions:
    build:
      context: .
      dockerfile: docker/Dockerfile.functions
    image: coffeenchill-functions:local
    depends_on:
      - azurite
    environment:
      - AzureWebJobsStorage=UseDevelopmentStorage=true
    ports:
      - "7071:80"
```

### Build and Run

```bash
# Build containers
docker-compose build

# Start services
docker-compose up

# Stop services
docker-compose down
```

## 9. Deployment

### Push to Docker Hub

```bash
docker login
docker tag coffeenchill-functions:v1.0 yourusername/coffeenchill-functions:v1.0
docker push yourusername/coffeenchill-functions:v1.0

docker tag coffeenchill-functions:v1.0 yourusername/coffeenchill-azure:v1.0
docker push yourusername/coffeenchill-azure:v1.0
```

### Deploy to Azure Functions

```bash
# Create Function App
az functionapp create \
  --resource-group myResourceGroup \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 10.0 \
  --functions-version 4 \
  --name coffeenchill-functions

# Deploy
func azure functionapp publish coffeenchill-functions --build remote
```

## 10. Team Contributions

| Team Member | Role | Contributions |
|------------|------|-----------------|
| ST10350498 - Full Stack Developer  Azure Functions implementation, Table Storage & File Share integration, API endpoints development, Docker containerization, comprehensive documentation 
  ST10492508 - azurite setup, menu crud functions, table entity design
**Work Summary:**
- Implemented 8 HTTP-triggered Azure Functions covering complete CRUD operations
- Integrated Azure Table Storage for menu items management
- Integrated Azure File Share for staff document storage
- Created complete Docker containerization with Azurite emulation
- Developed and tested all API endpoints with Postman
- Wrote comprehensive documentation and setup guides
- Set up local development environment with proper configuration

## 11. Video Demonstration

📹 **Part 1 Demonstration**: [YouTube Video Link - Unlisted]
(Coming Soon - Will include docker-compose execution and API testing)

**Video Contents:**
- Running standalone Docker containers (`docker run` and `docker-compose`)
- Creating and managing menu items via HTTP API
- Uploading and downloading staff documents
- Complete Postman collection execution
- Azure Storage emulation with Azurite
- All endpoints verified working correctly

## 12. Troubleshooting

### Connection Refused
**Problem**: Cannot connect to Azurite
**Solution**: Ensure Azurite is running and ports 10000-10002 are accessible. Check `local.settings.json` connection strings.

### Table Not Found
**Problem**: "Table not found" error
**Solution**: Tables are created automatically. Check startup logs for initialization messages.

### File Upload Fails
**Problem**: Document upload returns error
**Solution**: Verify file size and type (.pdf, .doc, .docx, .txt). Check file share permissions.

### Docker Container Exits
**Problem**: Container stops immediately
**Solution**: Check logs with `docker logs container_name`. Verify environment variables are set correctly.

## License

This project is part of the CLDV (Cloud Development) curriculum.

## Contact

For questions or support, refer to the course materials or contact the development team.
