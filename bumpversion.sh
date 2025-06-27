#!/bin/bash

# Path to your ProjectSettings.asset file
FILE="ProjectSettings/ProjectSettings.asset"

# Use sed to find the line and modify it
sed -i -E 's/^(\s*bundleVersion: [0-9]+\.[0-9]+\.)([0-9]+)/echo "\1$((\2+1))"/e' "$FILE"

echo "Patch version incremented successfully."