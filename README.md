# API Aggregation Service Documentation

## Overview

The **API Aggregation Service** fetches and aggregates data from multiple external APIs, including GitHub repositories, Twitter user data, and weather information from OpenWeatherMap. It offers a single endpoint for clients to retrieve consolidated information from these sources, with built-in fault tolerance that returns fallback data when any of the external services fail.

## Features
- **Data Aggregation:** Aggregates data from GitHub, Twitter, and a weather service into a single, cohesive response.
- **Caching:** Implements caching techniques to optimize performance and minimize redundant API calls, ensuring faster response times and reduced load on external services.
- **Filtering and Sorting:** Filters and sorts GitHub repositories based on criteria such as name, creation date, and last updated date, allowing for tailored data retrieval.
- **Fallback Mechanism:** Provides fallback data to ensure consistent output even when external services are unavailable or experience failures.
- **Configurable Inputs:** Supports configurable inputs for dynamic and flexible data retrieval, adapting to various use cases.
- **Performance Monitoring:** Tracks API response times and request counts, enabling performance monitoring and categorization into fast, average, and slow response buckets.
- **Asynchronous Processing:** Utilizes asynchronous tasks to efficiently handle multiple API calls concurrently, improving overall performance.


---

## API Endpoints

### 1. **GET /api/aggregation/aggregate**

#### Description:
This endpoint retrieves aggregated data from GitHub, Twitter, and Weather services, based on the input parameters, and returns a consolidated JSON response. In case of external API failure, the service returns predefined fallback data.

#### Request:
- **Method:** GET
- **Endpoint:** `/api/aggregation/aggregate`
- **Query Parameters:**

| Parameter       | Type     | Required | Description                                                                 |
|-----------------|----------|----------|-----------------------------------------------------------------------------|
| `github`        | string   | Yes      | The GitHub username to fetch repositories for.                              |
| `twitter`       | string   | Yes      | The Twitter username to fetch user data for.                                |
| `location`      | string   | Yes      | The location to fetch weather data for (e.g., city name).                   |
| `sortBy`        | string   | No       | The field to sort GitHub repositories by (e.g., `name`, `createddate`).     |
| `ascending`     | boolean  | No       | Sort order: `true` for ascending, `false` for descending (default: `true`). |
| `nameFilter`    | string   | No       | Filter GitHub repositories by name.                                         |
| `createdAfter`  | DateTime | No       | Filter GitHub repositories created after this date.                         |
| `createdBefore` | DateTime | No       | Filter GitHub repositories created before this date.                        |
| `updatedAfter`  | DateTime | No       | Filter GitHub repositories updated after this date.                         |
| `updatedBefore` | DateTime | No       | Filter GitHub repositories updated before this date.                        |

### Example Request:
```http
GET /api/aggregation/aggregate?github=testuser&twitter=testuser&location=Athens&sortBy=name&ascending=true&nameFilter=api&createdAfter=2023-01-01&createdBefore=2024-01-01
```
### Response

- **Status Codes:**
  - **200 OK** - Aggregated data successfully retrieved.
  - **400 Bad Request** - Invalid input parameters.
  - **500 Internal Server Error** - Error in external API calls or unexpected server error.
### Example Response

```json
{
  "gitHub": [
    {
      "name": "TestRepo",
      "html_url": "https://github.com/username/repository",
      "visibility": "public",
      "created_at": "2024-04-07T16:31:33Z",
      "updated_at": "2024-04-23T09:07:22Z"
    }
  ],
  "twitter": {
    "id": "1",
    "name": "TestUser",
    "username": "TestUsername",
  },
  "weather": {
    "name": "Athens",
    "main": {
      "temp": 301.2,
      "humidity": 74
    },
    "weather": [
      {
        "description": "clear sky"
      }
    ]
  }
}
```

### 2. **GET /api/aggregation/request-stats**

#### Description:
This endpoint provides detailed statistics on the number of requests and response times for each API (GitHub, Twitter, and Weather). The statistics include the total number of requests, response times, and performance buckets categorized by response time thresholds.

#### Example Request:
```http
GET /api/aggregation/request-stats
```
### Response:
- **Status Codes:**
  - **200 OK** - Statistics successfully retrieved.
  - **500 Internal Server Error** - Error retrieving statistics or unexpected server error.

### Example Response

```json
{
  "stats": {
    "github": {
      "totalRequests": 2,
      "responseTimes": [
        469,
        320
      ],
      "buckets": {
        "fast": 0,
        "average": 0,
        "slow": 2
      }
    },
    "twitter": {
      "totalRequests": 2,
      "responseTimes": [
        350,
        171
      ],
      "buckets": {
        "fast": 0,
        "average": 1,
        "slow": 1
      }
    },
    "weather": {
      "totalRequests": 2,
      "responseTimes": [
        311,
        74
      ],
      "buckets": {
        "fast": 1,
        "average": 0,
        "slow": 1
      }
    }
  }
}
```

### Response Structure

- **`stats`**: An object containing statistics for each API.

  - **`github`**: An object with statistics for the GitHub API.
    - **`totalRequests`**: `integer` - Total number of requests made to the GitHub API.
    - **`responseTimes`**: `array of integers` - List of response times (in milliseconds) for requests to GitHub.
    - **`buckets`**: `object` - Performance bucket counts.
      - **`fast`**: `integer` - Number of requests with response time < 100ms.
      - **`average`**: `integer` - Number of requests with response time between 100ms and 200ms.
      - **`slow`**: `integer` - Number of requests with response time > 200ms.

  - **`twitter`**: An object with statistics for the Twitter API, formatted similarly to GitHub.
    - **`totalRequests`**: `integer` - Total number of requests made to the Twitter API.
    - **`responseTimes`**: `array of integers` - List of response times (in milliseconds) for requests to Twitter.
    - **`buckets`**: `object` - Performance bucket counts.
      - **`fast`**: `integer` - Number of requests with response time < 100ms.
      - **`average`**: `integer` - Number of requests with response time between 100ms and 200ms.
      - **`slow`**: `integer` - Number of requests with response time > 200ms.

  - **`weather`**: An object with statistics for the Weather API, formatted similarly to GitHub.
    - **`totalRequests`**: `integer` - Total number of requests made to the Weather API.
    - **`responseTimes`**: `array of integers` - List of response times (in milliseconds) for requests to Weather.
    - **`buckets`**: `object` - Performance bucket counts.
      - **`fast`**: `integer` - Number of requests with response time < 100ms.
      - **`average`**: `integer` - Number of requests with response time between 100ms and 200ms.
      - **`slow`**: `integer` - Number of requests with response time > 200ms.


## Setup and Configuration

### Prerequisites

Before you start, make sure you have the following:

- **.NET 8.0 SDK** installed on your machine.
- **Environment Variables** set up for external API keys.

### Environment Variables

This API aggregation service requires access to several external APIs. You'll need to set up the following environment variables in a `.env` file in the root of your project:

```plaintext
TWITTER_BEARER_TOKEN=your_twitter_bearer_token_here
OPENWEATHERMAP_API_KEY=your_openweathermap_api_key_here
GITHUB_ACCESS_TOKEN=your_github_access_token_here
```

- **TWITTER_BEARER_TOKEN**: The Bearer token for accessing the Twitter API.
- **OPENWEATHERMAP_API_KEY**: The API key for accessing the OpenWeatherMap API.
- **GITHUB_ACCESS_TOKEN**: The access token for accessing the GitHub API.

### Installation
1. #### Restore the dependencies:
```bash
dotnet restore
```
2. #### Build the project:
```bash
dotnet build
```
3. #### Run the tests:
```bash
dotnet test
```
### Running the Service
1. #### Run the service locally:
```bash
dotnet run --launch-profile https
```
2. #### The API will be available at `https://localhost:7212` and has a fallback to `https://localhost:5064` if needed.

## License

#### API Aggregation Service is released under the MIT License. See the [LICENSE](https://github.com/AGXNT1337/api-aggregation-service/blob/main/LICENSE) file for details.