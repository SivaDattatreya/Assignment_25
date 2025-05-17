# ReqRes API Client

A .NET 6 class library that interacts with the ReqRes.in API to fetch, process, and cache user data.

## Project Structure

- **ReqResClient**: Core class library with the API client implementation
  - Models: Data transfer objects for API responses
  - Interfaces: Service contracts
  - Services: Implementation of API client and user service
  - Exceptions: Custom exception types
  - Configuration: Options classes
  - Extensions: Service registration helpers
- **ReqResClient.Tests**: Unit tests for the class library.  Here I have added test cases for both methods
- **ReqResClient.Demo**: Console app demonstrating usage. Through this console application, we will get data by both methods, have to run this by using 'Set as Startup project'

## How to Build and Run

### Prerequisites

- .NET 6.0 SDK or later

### Building the Solution

```bash
dotnet build