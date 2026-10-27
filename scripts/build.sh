#!/usr/bin/env bash
#
# build.sh - Build the PersonalWebsite solution
#
# Usage:
#   ./scripts/build.sh          # Build Release
#   ./scripts/build.sh --clean  # Clean then build
#   ./scripts/build.sh --debug  # Build Debug
#

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(dirname "$SCRIPT_DIR")"
CONFIGURATION="Release"
CLEAN=false

for arg in "$@"; do
    case $arg in
        --clean) CLEAN=true ;;
        --debug) CONFIGURATION="Debug" ;;
        *) echo "Unknown argument: $arg"; exit 1 ;;
    esac
done

if [[ "$CLEAN" == true ]]; then
    echo "Cleaning solution..."
    dotnet clean "$SOLUTION_DIR/PersonalWebsite.sln" --configuration "$CONFIGURATION"
fi

echo "Building solution ($CONFIGURATION)..."
dotnet build "$SOLUTION_DIR/PersonalWebsite.sln" --configuration "$CONFIGURATION" --no-restore

echo "Build completed successfully."