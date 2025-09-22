#!/bin/bash

# Remove any existing results directory to ensure a clean state
rm -rf ./TestResults

# Run tests with OpenCover format for SonarQube compatibility
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Begin SonarQube analysis with OpenCover format
dotnet sonarscanner begin /k:"your_key" \
    /d:sonar.host.url="http://localhost:9000" \
    /d:sonar.token="your_token" \
    /d:sonar.scanner.scanAll=false \
    /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" \
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/wwwroot/**,**/Data/**,**/Migrations/**,**/Program.cs,**/Dockerfile" \
    /d:sonar.test.exclusions="**/bin/**,**/obj/**"

# Build the project
dotnet build --no-restore

# End SonarQube analysis
dotnet sonarscanner end /d:sonar.token="your_token"