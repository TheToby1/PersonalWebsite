#!/usr/bin/env bash
#
# run-local.sh - Build and run the PersonalWebsite solution locally
#
# This script starts both the BackendFunctions (Azure Functions) and
# the Client (Blazor WebAssembly) projects. The Client is configured
# to call the Backend API at http://localhost:7071.
#
# Prerequisites:
#   - .NET 8.0 SDK (or later)
#   - Azure Functions Core Tools (func)
#   - curl (for health check)
#
# Usage:
#   ./run-local.sh          # Build and run both services
#   ./run-local.sh --build  # Build only, then run
#   ./run-local.sh --clean  # Clean, build, and run
#
set -euo pipefail

SOLUTION_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SOLUTION_DIR/BackendFunctions"
CLIENT_DIR="$SOLUTION_DIR/Client"

# --- Configuration ---
BACKEND_PORT=7071
CLIENT_PORT=5242
CLIENT_HTTPS_PORT=7177

# --- Colors ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log() { echo -e "${BLUE}[run-local]${NC} $1"; }
success() { echo -e "${GREEN}[run-local]${NC} $1"; }
warn() { echo -e "${YELLOW}[run-local]${NC} $1"; }
error() { echo -e "${RED}[run-local]${NC} $1" >&2; }

# --- Cleanup on exit ---
BACKEND_PID=""
CLIENT_PID=""

cleanup() {
    log "Shutting down services..."
    if [[ -n "$BACKEND_PID" ]] && kill -0 "$BACKEND_PID" 2>/dev/null; then
        kill "$BACKEND_PID" 2>/dev/null || true
        wait "$BACKEND_PID" 2>/dev/null || true
    fi
    if [[ -n "$CLIENT_PID" ]] && kill -0 "$CLIENT_PID" 2>/dev/null; then
        kill "$CLIENT_PID" 2>/dev/null || true
        wait "$CLIENT_PID" 2>/dev/null || true
    fi
    success "All services stopped."
}
trap cleanup EXIT INT TERM

# --- Check prerequisites ---
check_prerequisites() {
    local missing=()

    if ! command -v dotnet &>/dev/null; then
        missing+=("dotnet (.NET SDK)")
    fi
    if ! command -v func &>/dev/null; then
        missing+=("func (Azure Functions Core Tools)")
    fi
    if ! command -v curl &>/dev/null; then
        missing+=("curl")
    fi

    if [[ ${#missing[@]} -gt 0 ]]; then
        error "Missing required tools:"
        for tool in "${missing[@]}"; do
            error "  - $tool"
        done
        error ""
        error "Install .NET SDK: https://dotnet.microsoft.com/download"
        error "Install Azure Functions Core Tools: https://learn.microsoft.com/azure/azure-functions/functions-run-local"
        exit 1
    fi

    # Check .NET version
    local dotnet_version
    dotnet_version=$(dotnet --version 2>/dev/null | cut -d. -f1)
    if [[ "$dotnet_version" -lt 8 ]]; then
        error ".NET SDK 8.0 or later is required. Found version: $(dotnet --version)"
        exit 1
    fi

    success "Prerequisites check passed."
}

# --- Build ---
build_solution() {
    log "Building solution..."
    dotnet build "$SOLUTION_DIR/PersonalWebsite.sln" --configuration Release
    success "Build completed successfully."
}

# --- Restore LibMan (Bootstrap CSS) ---
restore_libman() {
    log "Restoring LibMan packages (Bootstrap CSS)..."
    # LibMan packages are restored during build via Microsoft.Web.LibraryManager.Build
    # The bootstrap CSS should already be in Client/wwwroot/lib/bootstrap/
    if [[ ! -d "$CLIENT_DIR/wwwroot/lib/bootstrap" ]]; then
        warn "Bootstrap lib not found. Running libman restore..."
        if command -v libman &>/dev/null; then
            (cd "$CLIENT_DIR" && libman restore)
        else
            warn "libman CLI not found. Bootstrap will be restored during build."
        fi
    fi
    success "LibMan packages ready."
}

# --- Start Backend (Azure Functions) ---
start_backend() {
    log "Starting BackendFunctions (Azure Functions) on port $BACKEND_PORT..."
    # Run from the output directory where .azurefunctions folder is located
    # Copy local.settings.json to output directory if it exists
    if [[ -f "$BACKEND_DIR/local.settings.json" ]]; then
        cp "$BACKEND_DIR/local.settings.json" "$BACKEND_DIR/bin/Release/net8.0/local.settings.json"
    fi
    (cd "$BACKEND_DIR/bin/Release/net8.0" && func start --port "$BACKEND_PORT" --no-build) &
    BACKEND_PID=$!
    log "Backend PID: $BACKEND_PID"

    # Wait for backend to be ready
    log "Waiting for backend to be ready..."
    local max_wait=60
    local waited=0
    while true; do
        # Check if the backend responds with a valid response (200 = healthy, 400 = validation error but endpoint works)
        # Send a valid ContactRequest to get a proper 200 response
        if curl -s -o /dev/null -w "%{http_code}" "http://localhost:$BACKEND_PORT/api/sendcommunication" \
            -X POST -H "Content-Type: application/json" \
            -d '{"from":"healthcheck@local","subject":"Health Check","content":"ping"}' 2>/dev/null | grep -q "200"; then
            break
        fi
        sleep 2
        waited=$((waited + 2))
        if [[ $waited -ge $max_wait ]]; then
            warn "Backend may not be fully ready after ${max_wait}s, continuing anyway..."
            break
        fi
        # Check if process is still alive
        if ! kill -0 "$BACKEND_PID" 2>/dev/null; then
            error "Backend process exited unexpectedly."
            exit 1
        fi
    done
    success "Backend is running on http://localhost:$BACKEND_PORT"
}

# --- Start Client (Blazor WebAssembly) ---
start_client() {
    log "Starting Client (Blazor WebAssembly) on port $CLIENT_PORT..."
    (cd "$CLIENT_DIR" && dotnet run --configuration Release --project "$CLIENT_DIR/PersonalWebsite.Client.csproj" --urls "http://localhost:$CLIENT_PORT;https://localhost:$CLIENT_HTTPS_PORT") &
    CLIENT_PID=$!
    log "Client PID: $CLIENT_PID"

    # Wait for client to be ready
    log "Waiting for client to be ready..."
    local max_wait=60
    local waited=0
    while ! curl -s "http://localhost:$CLIENT_PORT" 2>/dev/null | grep -q "app\|blazor\|html" 2>/dev/null; do
        sleep 2
        waited=$((waited + 2))
        if [[ $waited -ge $max_wait ]]; then
            warn "Client may not be fully ready after ${max_wait}s, continuing anyway..."
            break
        fi
        # Check if process is still alive
        if ! kill -0 "$CLIENT_PID" 2>/dev/null; then
            error "Client process exited unexpectedly."
            exit 1
        fi
    done
    success "Client is running on http://localhost:$CLIENT_PORT"
}

# --- Main ---
main() {
    local clean_build=false
    local build_only=false

    for arg in "$@"; do
        case $arg in
            --clean) clean_build=true ;;
            --build) build_only=true ;;
            *) warn "Unknown argument: $arg" ;;
        esac
    done

    check_prerequisites

    if [[ "$clean_build" == true ]]; then
        log "Cleaning solution..."
        dotnet clean "$SOLUTION_DIR/PersonalWebsite.sln" --configuration Release
    fi

    restore_libman
    build_solution

    if [[ "$build_only" == true ]]; then
        success "Build only mode. Exiting."
        exit 0
    fi

    log "============================================"
    log "  PersonalWebsite - Local Development"
    log "============================================"
    log ""
    log "  Backend API:  http://localhost:$BACKEND_PORT"
    log "  Client App:   http://localhost:$CLIENT_PORT"
    log "  Client (HTTPS): https://localhost:$CLIENT_HTTPS_PORT"
    log ""
    log "  Press Ctrl+C to stop all services."
    log "============================================"
    echo ""

    start_backend
    start_client

    success "All services are running!"
    log "Open http://localhost:$CLIENT_PORT in your browser."
    log ""

    # Keep the script running until interrupted
    wait
}

main "$@"
