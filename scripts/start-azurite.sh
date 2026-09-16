#!/usr/bin/env bash
# Start Azurite (Azure Storage Emulator) for local development
# Creates a Docker container with all storage services

set -euo pipefail

echo "=========================================="
echo "Starting Azurite Storage Emulator..."
echo "=========================================="
echo ""

# Check if Docker is running
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker is not installed or running"
    exit 1
fi

# Stop existing container if running
if docker ps -a --format '{{.Names}}' | grep -q '^coffeenchill-azurite$'; then
    echo "Stopping existing Azurite container..."
    docker stop coffeenchill-azurite 2>/dev/null || true
fi

echo "Starting Azurite container..."
docker run -d \
  --name coffeenchill-azurite \
  -p 10000:10000 \
  -p 10001:10001 \
  -p 10002:10002 \
  -v azurite-data:/data \
  mcr.microsoft.com/azure-storage/azurite:3.20.0

echo ""
echo "=========================================="
echo "Azurite started successfully!"
echo "=========================================="
echo "Blob Storage:  http://127.0.0.1:10000"
echo "Queue Storage: http://127.0.0.1:10001"
echo "File Share:    http://127.0.0.1:10002"
echo ""
echo "Management tools:"
echo "  Stop:    docker stop coffeenchill-azurite"
echo "  Restart: docker restart coffeenchill-azurite"
echo "  Remove:  docker rm coffeenchill-azurite"
echo "  View logs: docker logs -f coffeenchill-azurite"
