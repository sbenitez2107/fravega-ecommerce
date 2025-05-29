# fravega-ecommerce
Technical Challenge Specification

## Prerequisites
Before running the project, ensure you have the following installed:

* .NET SDK (version compatible with the project)
* Docker (for MongoDB container)
* Visual Studio (recommended, with .NET development tools)
* MongoDB Docker Image (or local MongoDB instance)
* Postman (optional, for testing the API endpoints)

## Getting Started
Follow these steps to run the project locally:

Clone the repository:

git clone https://github.com/sbenitez2107/fravega-ecommerce.git
cd fravega-ecommerce
Start MongoDB container (if using Docker):

## 1) Start up application and mongoDB using docker compose
docker-compose up -d --build //Build API Rest docker image and then startup microservice container 

Check the path http://localhost:5000/swagger/index.html to see if swagger is active.

Use the command docker-compose down for stopping the execution

## 2) Start up application using Visual Studio
To build and run the project in Visual Studio, first clean the solution, then select Build Solution. Once all the projects have been successfully compiled, run the HTTPS profile.

## Start up only mongoDB for testing
To use the MongoDB database, run the following command to create a container with MongoDB. The collections that will be created are orders and counters, which are used to manage the ID fields of the tables. This database is also used for running unit and integration tests.

docker-compose -f .\docker-compose-mongo-db.yml up -d

## 3) Using Postman collection
For API testing purposes, a Postman collection with the relevant methods and endpoints is also included.

Fravega Collection.postman_collection.json

## 4) Execute suite test
# How to Run the Test Suite Using Visual Studio
To execute the unit and integration tests for the project, follow these steps:

Open the solution in Visual Studio and go to the Test menu and select Test Explorer.
Alternatively, you can open it via: View > Test Explorer.

In the Test Explorer window, click on Run All to execute all available tests.
Make sure the MongoDB container is up and running before executing the tests, as it is required for both unit and integration test scenarios.

## How to Run the Test Suite from the Command Line
To run the unit tests using the .NET CLI, follow these steps:

Open a terminal or command prompt, navigate to the root folder of the test project /FravegaEcommerceAPI/test/FravegaEcommerce.Tests

Run the following command, this will build the test project:
dotnet test --filter "TestCategory=ApiTest" --results-directory ./TestResults
dotnet test -- -parallel none --filter "Collection=ApiTest"

The result testing applied must be like this:

Test summary: total: 50, failed: 0, succeeded: 50, skipped: 0, duration: 8.7s

## Project Overview
The project is organized into a layered modular structure, including Controllers, Services, and Repositories. It also incorporates additional services such as model and business validators (for state transitions) and a text translation service, which could be enhanced by integrating an internationalization library if needed.

Mapping classes are used for model type conversion, improving the clarity and maintainability of the design.

Idempotent message handling was added for operations such as adding a new event, ensuring safe retries without duplication.

Clear and structured message logging is implemented. Additionally, logs can be recorded per API call and stored in a database, an S3 bucket in the cloud, or integrated with monitoring systems such as CloudWatch or Prometheus.

## Potential Improvements
Implement OAuth authentication using an external provider.

Deploy the application to the cloud for scalability and availability.

Enhance message content to be more descriptive and informative.

Improve Swagger documentation for better API visibility and developer experience.



