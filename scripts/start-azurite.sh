#!/usr/bin/env bash
# Simple script to run Azurite using Docker

CONTAINER_NAME=coffeenchill-azurite
IMAGE=mcr.microsoft.com/azure-storage/azurite:3.20.0

docker pull ${IMAGE}

docker run -d --name ${CONTAINER_NAME} -p 10000:10000 -p 10001:10001 -p 10002:10002 ${IMAGE}

echo "Azurite started in container ${CONTAINER_NAME}"
