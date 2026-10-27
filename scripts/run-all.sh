#!/usr/bin/env bash
#
# run-all.sh - Build and run both Backend and Frontend
#
# Usage:
#   ./scripts/run-all.sh              # Build and run both
#   ./scripts/run-all.sh --no-build   # Run only (assumes already built)
#   ./scripts/run-all.sh --clean      # Clean build then run
#

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(dirname "$SCRIPT_DIR")"

BUILD=true
CLEAN=false

for arg in "$@"; do
    case $arg in
        --no-build) BUILD=false ;;
        --clean) CLEAN=true ;;
        *) echo "Unknown argument: $arg"; exit 1 ;;
    esac
done

# Build if requested
if [[ "$BUILD" == true ]]; then
    BUILD_ARGS=()
    [[ "$CLEAN" == true ]] && BUILD_ARGS+=(--clean)
    "$SCRIPT_DIR/build.sh" "${BUILD_ARGS[@]}"
fi

echo ""
echo "============================================"
echo "  PersonalWebsite - Local Development"
echo "============================================"
echo ""
echo "  Backend API:  http://localhost:7071"
echo "  Frontend:     http://localhost:5242"
echo "  Frontend HTTPS: https://localhost:7177"
echo ""
echo "  Press Ctrl+C to stop all services."
echo "============================================"
echo ""

# Start backend in background
"$SCRIPT_DIR/run-backend.sh" &
BACKEND_PID=$!

# Give backend a moment to start
sleep 3

# Start frontend (this will block)
"$SCRIPT_DIR/run-frontend.sh" &
FRONTEND_PID=$!

# Cleanup on exit
cleanup() {
    echo ""
    echo "Shutting down services..."
    kill $BACKEND_PID 2>/dev/null || true
    kill $FRONTEND_PID 2>/dev/null || true
    wait $BACKEND_PID 2>/dev/null || true
    wait $FRONTEND_PID 2>/dev/null || true
    echo "All services stopped."
}
trap cleanup EXIT INT TERM

# Wait for both processes
wait