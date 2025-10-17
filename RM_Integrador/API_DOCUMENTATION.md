# 📚 API Documentation

## 🔑 Authentication

All API endpoints require authentication. Use the following header:

```
Authorization: Bearer {your-token}
```

## 🛣️ Endpoints

### DataServer Operations

#### GET /api/dataserver/{serverName}
Retrieves data from specified DataServer.

**Parameters:**
```json
{
  "filter": "string (optional) - SQL-like filter",
  "fields": "array (optional) - Specific fields to return",
  "pageSize": "number (optional) - Results per page",
  "pageNumber": "number (optional) - Page number"
}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "field1": "value1",
      "field2": "value2"
    }
  ],
  "totalRecords": 1,
  "pageSize": 10,
  "currentPage": 1
}
```

#### POST /api/dataserver/{serverName}
Creates new record in specified DataServer.

**Request Body:**
```json
{
  "field1": "value1",
  "field2": "value2"
}
```

**Response:**
```json
{
  "success": true,
  "id": "newly-created-id",
  "message": "Record created successfully"
}
```

### Template Management

#### GET /api/templates
Retrieves all available templates.

**Response:**
```json
{
  "templates": [
    {
      "id": "template-id",
      "name": "Template Name",
      "description": "Template Description",
      "dataServer": "ServerName",
      "type": "GET/POST"
    }
  ]
}
```

#### POST /api/templates
Creates a new template.

**Request Body:**
```json
{
  "name": "Template Name",
  "description": "Template Description",
  "dataServer": "ServerName",
  "type": "GET/POST",
  "configuration": {
    "filter": "ACTIVE = 1",
    "fields": ["FIELD1", "FIELD2"]
  }
}
```

### JSON Comparison

#### POST /api/compare
Compares two JSON structures.

**Request Body:**
```json
{
  "source": {
    "field1": "value1"
  },
  "target": {
    "field1": "different-value"
  }
}
```

**Response:**
```json
{
  "differences": [
    {
      "path": "field1",
      "sourceValue": "value1",
      "targetValue": "different-value",
      "type": "value-mismatch"
    }
  ]
}
```

## 📝 Error Handling

All endpoints return standard error responses:

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": "Additional error details"
  }
}
```

Common error codes:
- `AUTH_REQUIRED`: Authentication required
- `INVALID_REQUEST`: Invalid request parameters
- `NOT_FOUND`: Resource not found
- `SERVER_ERROR`: Internal server error

## 🔄 Rate Limiting

- Rate limit: 100 requests per minute
- Headers included in response:
  - `X-RateLimit-Limit`
  - `X-RateLimit-Remaining`
  - `X-RateLimit-Reset`