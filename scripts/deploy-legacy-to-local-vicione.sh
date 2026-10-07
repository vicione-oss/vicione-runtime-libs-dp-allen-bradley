#!/bin/bash
# Deploy AllenBradley.Legacy to local Vicione installation
# Usage: deploy-legacy-to-local-vicione.sh <vo-suite-folder> [Release] [runtime-identifier]
#   Parameter 2: If "Release", builds in Release mode, otherwise Debug (default)
#   Parameter 3: Runtime identifier (e.g., win-x64, linux-x64, linux-arm64). If not specified, publishes framework-dependent.

if [ -z "$1" ]; then
    echo "Error: vo-suite folder not specified"
    echo "Usage: deploy-legacy-to-local-vicione.sh <vo-suite-folder> [Release] [runtime-identifier]"
    exit 1
fi

VO_SUITE_FOLDER="$1"
BUILD_CONFIG="Debug"
RUNTIME_ID=""

# Check if second parameter is a runtime identifier or build config
case "$2" in
    win-x64|linux-x64|linux-arm64)
        RUNTIME_ID="$2"
        ;;
    Release|Debug)
        BUILD_CONFIG="$2"
        ;;
esac

# If third parameter exists, it's the runtime identifier
if [ -n "$3" ]; then
    RUNTIME_ID="$3"
fi
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$SCRIPT_DIR/.."

# Read version from VERSION file (handle BOM and whitespace)
VERSION_FILE="$SOLUTION_ROOT/VERSION"
if [ ! -f "$VERSION_FILE" ]; then
    echo "Error: VERSION file not found: $VERSION_FILE"
    exit 1
fi
# Remove BOM (EF BB BF) if present, then trim whitespace
VERSION=$(sed '1s/^\xEF\xBB\xBF//' "$VERSION_FILE" | tr -d '[:space:]')
echo "Using version: $VERSION"
echo ""

if [ -z "$RUNTIME_ID" ]; then
    SOURCE_FOLDER="$SCRIPT_DIR/../src/AllenBradley.Legacy/bin/$BUILD_CONFIG/net10.0/publish"
else
    SOURCE_FOLDER="$SCRIPT_DIR/../src/AllenBradley.Legacy/bin/$BUILD_CONFIG/net10.0/$RUNTIME_ID/publish"
fi
TARGET_FOLDER="$VO_SUITE_FOLDER/src/Core.OS/bin/Debug/net10.0/Cache_Standalone/ViciOne.Suite.ClusterManagement/Dependencies/ViciOne.Suite.DataPort.AllenBradley.Legacy/$VERSION"

if [ ! -d "$VO_SUITE_FOLDER" ]; then
    echo "Error: vo-suite folder does not exist: $VO_SUITE_FOLDER"
    exit 1
fi

if [ -z "$RUNTIME_ID" ]; then
    echo "Building and publishing AllenBradley.Legacy project in $BUILD_CONFIG mode (framework-dependent)..."
else
    echo "Building and publishing AllenBradley.Legacy project in $BUILD_CONFIG mode for $RUNTIME_ID..."
fi
echo ""
cd "$SOLUTION_ROOT"
echo "Cleaning project..."
dotnet clean src/AllenBradley.Legacy/AllenBradley.Legacy.csproj -c $BUILD_CONFIG
if [ $? -ne 0 ]; then
    echo ""
    echo "Error: Clean failed"
    exit 1
fi
echo ""
if [ -z "$RUNTIME_ID" ]; then
    dotnet publish src/AllenBradley.Legacy/AllenBradley.Legacy.csproj -c $BUILD_CONFIG
else
    dotnet publish src/AllenBradley.Legacy/AllenBradley.Legacy.csproj -c $BUILD_CONFIG -r $RUNTIME_ID --no-self-contained
fi
if [ $? -ne 0 ]; then
    echo ""
    echo "Error: Build/publish failed"
    exit 1
fi
echo ""

if [ ! -d "$SOURCE_FOLDER" ]; then
    echo "Error: Source folder does not exist: $SOURCE_FOLDER"
    echo "Publish may have failed."
    exit 1
fi

if [ ! -d "$TARGET_FOLDER" ]; then
    echo "Target folder does not exist, creating: $TARGET_FOLDER"
    mkdir -p "$TARGET_FOLDER"
    if [ ! -d "$TARGET_FOLDER" ]; then
        echo "Error: Could not create target folder"
        exit 1
    fi
    echo ""
fi

echo "Copying files from:"
echo "  $SOURCE_FOLDER"
echo "To:"
echo "  $TARGET_FOLDER"
echo ""

cp -rf "$SOURCE_FOLDER/"* "$TARGET_FOLDER/"

if [ $? -eq 0 ]; then
    echo ""
    echo "Deployment successful!"
else
    echo ""
    echo "Deployment failed with error code $?"
    exit $?
fi
