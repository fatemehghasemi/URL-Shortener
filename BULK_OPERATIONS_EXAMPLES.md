# Bulk Operations API Examples

## Bulk Create Short URLs

Create multiple short URLs in a single request.

### Endpoint
```
POST /bulk/shorten
```

### Request Body
```json
{
  "urls": [
    {
      "url": "https://www.google.com",
      "expiresAt": "2024-12-31T23:59:59Z"
    },
    {
      "url": "https://github.com/microsoft/dotnet"
    },
    {
      "url": "https://stackoverflow.com/questions/tagged/asp.net-core",
      "expiresAt": "2024-06-30T12:00:00Z"
    }
  ]
}
```

### Response
```json
{
  "createdLinks": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "originalUrl": "https://www.google.com",
      "shortCode": "aB3xY9",
      "shortUrl": "http://localhost:5000/aB3xY9",
      "createdAt": "2024-01-30T10:30:00Z",
      "expiresAt": "2024-12-31T23:59:59Z"
    },
    {
      "id": "456e7890-e89b-12d3-a456-426614174001",
      "originalUrl": "https://github.com/microsoft/dotnet",
      "shortCode": "Km8pQ2",
      "shortUrl": "http://localhost:5000/Km8pQ2",
      "createdAt": "2024-01-30T10:30:01Z",
      "expiresAt": null
    }
  ],
  "failedLinks": [
    {
      "originalUrl": "https://stackoverflow.com/questions/tagged/asp.net-core",
      "error": "Expiration date must be in the future"
    }
  ],
  "successCount": 2,
  "failureCount": 1
}
```

### Features
- **Batch Processing**: Create up to 100 URLs in one request
- **Partial Success**: Some URLs can succeed while others fail
- **Transaction Safety**: All successful URLs are saved in one database transaction
- **Validation**: Each URL is validated individually
- **Optional Expiration**: Set expiration dates per URL

---

## Bulk Get Analytics

Get analytics data for multiple short URLs in a single request.

### Endpoint
```
POST /bulk/analytics
```

### Request Body
```json
{
  "shortCodes": ["aB3xY9", "Km8pQ2", "nonexistent"]
}
```

### Response
```json
{
  "analytics": [
    {
      "linkId": "123e4567-e89b-12d3-a456-426614174000",
      "shortCode": "aB3xY9",
      "originalUrl": "https://www.google.com",
      "totalClicks": 45,
      "createdAt": "2024-01-30T10:30:00Z",
      "lastClickAt": "2024-01-30T15:45:30Z",
      "isActive": true,
      "expiresAt": "2024-12-31T23:59:59Z",
      "topReferrers": [
        {
          "referer": "https://twitter.com",
          "count": 12
        },
        {
          "referer": "https://facebook.com",
          "count": 8
        }
      ],
      "last7Days": [
        {
          "date": "2024-01-24T00:00:00Z",
          "clicks": 5
        },
        {
          "date": "2024-01-25T00:00:00Z",
          "clicks": 12
        },
        {
          "date": "2024-01-26T00:00:00Z",
          "clicks": 8
        }
      ]
    },
    {
      "linkId": "456e7890-e89b-12d3-a456-426614174001",
      "shortCode": "Km8pQ2",
      "originalUrl": "https://github.com/microsoft/dotnet",
      "totalClicks": 23,
      "createdAt": "2024-01-30T10:30:01Z",
      "lastClickAt": "2024-01-30T14:20:15Z",
      "isActive": true,
      "expiresAt": null,
      "topReferrers": [
        {
          "referer": "https://reddit.com",
          "count": 15
        }
      ],
      "last7Days": [
        {
          "date": "2024-01-29T00:00:00Z",
          "clicks": 10
        },
        {
          "date": "2024-01-30T00:00:00Z",
          "clicks": 13
        }
      ]
    }
  ],
  "notFound": [
    {
      "shortCode": "nonexistent",
      "error": "Short URL not found"
    }
  ],
  "successCount": 2,
  "notFoundCount": 1
}
```

### Features
- **Batch Analytics**: Get data for up to 50 short codes in one request
- **Optimized Queries**: Single database query for all links and their analytics
- **Partial Results**: Returns data for found links, reports missing ones
- **Rich Analytics**: Includes click counts, referrers, and daily trends
- **Memory Efficient**: Limits click log data to prevent memory issues

---

## Use Cases

### Marketing Campaigns
```bash
# Create multiple campaign URLs at once
curl -X POST http://localhost:5000/bulk/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "urls": [
      {"url": "https://example.com/campaign1", "expiresAt": "2024-03-31T23:59:59Z"},
      {"url": "https://example.com/campaign2", "expiresAt": "2024-03-31T23:59:59Z"},
      {"url": "https://example.com/campaign3", "expiresAt": "2024-03-31T23:59:59Z"}
    ]
  }'
```

### Analytics Dashboard
```bash
# Get analytics for all campaign URLs
curl -X POST http://localhost:5000/bulk/analytics \
  -H "Content-Type: application/json" \
  -d '{
    "shortCodes": ["aB3xY9", "Km8pQ2", "9zX4mN"]
  }'
```

### Data Migration
```bash
# Migrate existing URLs from another service
curl -X POST http://localhost:5000/bulk/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "urls": [
      {"url": "https://old-service.com/redirect1"},
      {"url": "https://old-service.com/redirect2"},
      {"url": "https://old-service.com/redirect3"}
    ]
  }'
```

---

## Error Handling

### Validation Errors
- Invalid URL format
- Expiration date in the past
- Empty or null URLs
- Batch size limits exceeded

### Partial Success
Both bulk operations support partial success - some items can succeed while others fail. This allows for robust batch processing where a few invalid items don't prevent the entire batch from processing.

### Rate Limiting
- Bulk create: Maximum 100 URLs per request
- Bulk analytics: Maximum 50 short codes per request
- These limits prevent memory issues and ensure good performance

---

## Performance Notes

- **Database Optimization**: Uses efficient batch queries and proper indexing
- **Memory Management**: Limits result sets to prevent memory issues
- **Transaction Safety**: All successful operations are committed in a single transaction
- **Error Isolation**: Individual item failures don't affect other items in the batch