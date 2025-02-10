# Aggregator API

## Overview

Aggregator API provides aggregated data for a given city, including:

- **Weather Information** (temperature, condition, wind speed, humidity).
- **Top Tourist Attractions** and notable places.
- **Latest News Articles** related to the city.

## Installation & Setup

### **1. Clone the repository**

```sh
git clone https://github.com/carvex21/apiaggregator.git
cd apiaggregator
```

### **2. Configure Environment Variables**

Set up `appsettings.json` with API keys:

```json
{
  "ApiSettings": {
    "GeoapifyApiKey": "your_Geoapify_api_key",
    "WeatherApiKey": "your_Weather_api_key",
    "NewsApiKey": "your_News_api_key"
  }
}
```

### **3. Run the API**

```sh
dotnet run
```

---

## API Documentation

### **Base URL**

```
https://localhost:7248
```

### **1. Get Aggregated City Data**

#### **Endpoint:**

```
GET /api/aggregator/data?city={city_name}
```

Retrieves weather, places, and news for the given city.

#### **Request Example**

```http
GET https://localhost:7248/api/aggregator/data?city=athens
```

#### **Response Example**

```json
{
  "location": {
    "city": "Athens",
    "latitude": 37.9755648,
    "longitude": 23.7348324
  },
  "weather": {
    "temperature": 4.96,
    "condition": "scattered clouds",
    "windSpeed": 3.58,
    "humidity": 66
  },
  "news": [
    {
      "author": "Karla Ward",
      "title": "Major construction project coming soon to Richmond Road in Lexington.",
      "description": "What’s an R-CUT, and when will the commute on Richmond Road be affected?",
      "url": "https://www.kentucky.com/news/local/counties/fayette-county/article297120099.html",
      "publishedAt": "2025-02-05T15:59:35+01:00"
    }
  ],
  "places": [
    {
      "name": "Πλατεία Συντάγματος",
      "category": "highway",
      "address": "Syntagma Square, Βασιλίσσης Αμαλίας, 105 57 Athens, Greece"
    }
  ]
}
```

### **Error Handling**

| Error Code | Message                   | Description                           |
| ---------- | ------------------------- | ------------------------------------- |
| 400        | `"Bad Request"`           | Invalid or missing parameters.        |
| 404        | `"City not found"`        | No data found for the specified city. |
| 500        | `"Internal Server Error"` | Unexpected server failure.            |

---

## Contributing

Feel free to submit issues or open a pull request.

## License

MIT License