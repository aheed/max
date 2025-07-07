#!/bin/bash

# Paths to your ProjectSettings.asset files
FILES=("ProjectSettings/ProjectSettings.asset" "Assets/Settings/Build Profiles/AWS Release.asset" "Assets/Settings/Build Profiles/Crazygames Release.asset") # Add other file paths if needed

# Get the current patch number from the main file
main_file="ProjectSettings/ProjectSettings.asset"
current_patch=$(grep -o -E 'bundleVersion: [0-9]+\.[0-9]+\.[0-9]+' "$main_file" | grep -o -E '[0-9]+$')

# Increment the patch number
new_patch=$((current_patch + 1))

# Function to update the patch version
update_patch_version() {
  local file=$1
  
  # Use sed to update the patch version, considering any leading characters
  sed -i -E "s/(.*bundleVersion: [0-9]+\.[0-9]+\.)[0-9]+/\1$new_patch/" "$file"
}

# Apply the change to each specified file
for file in "${FILES[@]}"; do
  update_patch_version "$file"
done

echo "Patch versions updated successfully to $new_patch."