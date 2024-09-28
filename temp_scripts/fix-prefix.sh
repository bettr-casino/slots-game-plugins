#!/bin/bash

# Check if the required arguments are passed
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
    echo "Usage: $0 /path/to/directory old_pattern new_pattern"
    exit 1
fi

# Set the directory and patterns from the arguments
DIRECTORY="$1"
OLD_PATTERN="$2"
NEW_PATTERN="$3"

# Loop through all files in the directory
for file in "$DIRECTORY"/*; do
    # Extract the base name of the file (without directory path)
    filename=$(basename "$file")

    # Check if the filename starts with the new pattern
    if [[ $filename != ${NEW_PATTERN}* ]]; then
        # Check if it starts with the old pattern, and replace it with the new pattern
        if [[ $filename == ${OLD_PATTERN}* ]]; then
            new_filename=$(echo "$filename" | sed "s/^${OLD_PATTERN}/${NEW_PATTERN}/")
            # Rename the file
            mv "$file" "$DIRECTORY/$new_filename"
            echo "Renamed $filename to $new_filename"
        fi
    fi
done
