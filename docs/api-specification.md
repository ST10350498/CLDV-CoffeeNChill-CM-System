# CoffeeNChill API specification (Part 1)

This document contains a short summary of the HTTP API endpoints used in Part 1 of the CoffeeNChill project.

## Base URL
- Local (dev): http://localhost:7071/api

## Endpoints

- GET /api/menu
  - Returns all menu items.

- GET /api/menu/category/{category}
  - Returns menu items filtered by category (PartitionKey).

- POST /api/menu
  - Create a new menu item (JSON body with category, sku, name, description, price, isAvailable).

- PUT /api/menu/{category}/{id}
  - Update an existing menu item.

- DELETE /api/menu/{category}/{id}
  - Delete a menu item.

- POST /api/documents/upload
  - Upload a staff document (multipart/form-data, field `file`).

- GET /api/documents/download/{fileName}
  - Download a staff document by filename.

More details and example request/response bodies should be added to this file as the API stabilises.
