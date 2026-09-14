#!/usr/bin/env bash
# Build script for the Functions project

set -euo pipefail

echo "Restoring and building solution..."
dotnet restore
dotnet build -c Release

echo "Publishing functions app to ./publish"
dotnet publish -c Release -o publish

echo "Build complete. Output: ./publish"
