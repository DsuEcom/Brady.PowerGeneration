Power Generation XML Processor
Overview
A .NET application that monitors and processes power generation data from XML files. The application calculates generation values, emissions, and heat rates based on provided reference data.
Features

Automatic XML file monitoring
Generation value calculations
Emissions tracking
Heat rate calculations
Real-time file processing

Getting Started
Prerequisites

.NET 8.0 SDK
Visual Studio 2022 (or preferred IDE)

Installation

Clone the repository
git clone [repository-url]
Build the solution
dotnet build
Configure paths in appsettings.json:
{
  "PowerGeneration": {
    "InputFolderPath": "<<inputFilePath>>",
    "OutputFolderPath": "<<outputFilePath>>",
    "ReferenceDataPath": "<<referenceDataFilePath>>"
  }
}

Running the Application
dotnet run --project Brady.PowerGeneration.Console
Usage

Place XML files in the input folder
Application automatically processes files
Results are saved in the output folder with -Result.xml suffix

Testing
Run the tests using:
dotnet test
Project Structure
Brady.PowerGeneration/
├── src/
│   ├── Core/                # Business logic
│   ├── Infrastructure/      # External concerns
│   └── Console/            # Application entry
└── tests/
└── Tests/              # Unit tests
