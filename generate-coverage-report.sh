#!/bin/bash

dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

reportgenerator

# Open report in browser (macOS)
if command -v open &> /dev/null; then
    open ./CoverageReport/index.html
fi
