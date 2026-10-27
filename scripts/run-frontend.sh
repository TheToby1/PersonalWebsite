#!/usr/bin/env bash
#
# run-frontend.sh - Run the Client (Blazor WebAssembly) locally
#
# Prerequisites:
#   - Build must have been run first (./scripts/build.sh)
#
# Usage:
#   ./scripts/run-frontend.sh                    # Run on default ports
#   ./scripts/run-frontend.sh --http-port 5242 --https-port 7177  # Custom ports
#

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(dirname "$SCRIPT_DIR")"
CLIENT_DIR="$SOLUTION_DIR/Client"
HTTP_PORT=5242
HTTPS_PORT=7177

while [[ $# -gt 0 ]]; do
    case $1 in
        --http-port) HTTP_PORT="$2"; shift 2 ;;
        --https-port) HTTPS_PORT="$2"; shift 2 ;;
        *) echo "Unknown argument: $1"; exit 1 ;;
    esac
done

# Check if build output exists
if [[ ! -d "$CLIENT_DIR/bin/Release/net8.0" ]]; then
    echo "Error: Build output not found. Run ./scripts/build.sh first."
    exit 1
fi

echo "Starting Client (Blazor WASM) on:"
echo "  HTTP:  http://localhost:$HTTP_PORT"
echo "  HTTPS: https://localhost:$HTTPS_PORT"
echo "Press Ctrl+C to stop"
echo ""

cd "$CLIENT_DIR"
dotnet run --configuration Release --project "$CLIENT_DIR/PersonalWebsite.Client.csproj" --urls "http://localhost:$HTTP_PORT;https://localhost:$HTTPS_PORT"