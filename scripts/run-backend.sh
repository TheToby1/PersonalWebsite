#!/usr/bin/env bash
#
# run-backend.sh - Run the BackendFunctions (Azure Functions) locally
#
# Prerequisites:
#   - Azure Functions Core Tools (func)
#   - Build must have been run first (./scripts/build.sh)
#
# Usage:
#   ./scripts/run-backend.sh          # Run on default port 7071
#   ./scripts/run-backend.sh --port 7072  # Run on custom port
#

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(dirname "$SCRIPT_DIR")"
BACKEND_DIR="$SOLUTION_DIR/BackendFunctions"
PORT=7071

for arg in "$@"; do
    case $arg in
        --port) PORT="$2"; shift 2 ;;
        *) echo "Unknown argument: $arg"; exit 1 ;;
    esac
done

# Check if build output exists
if [[ ! -d "$BACKEND_DIR/bin/Release/net8.0" ]]; then
    echo "Error: Build output not found. Run ./scripts/build.sh first."
    exit 1
fi

# Check for func CLI
if ! command -v func &>/dev/null; then
    echo "Error: Azure Functions Core Tools (func) not found."
    echo "Install: https://learn.microsoft.com/azure/azure-functions/functions-run-local"
    exit 1
fi

echo "Starting BackendFunctions on http://localhost:$PORT"
echo "Press Ctrl+C to stop"
echo ""

# Run from the project directory - func will find the built output
cd "$BACKEND_DIR"
func start --port "$PORT" --no-build