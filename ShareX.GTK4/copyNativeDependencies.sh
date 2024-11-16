#!/bin/sh

# Usage: ./copyNativeDependencies.sh /path/to/libadwaita-1.so.0 /path/to/output/dir

LIB_PATH=$1
OUTPUT_DIR=$2

# Recursively copy dependencies
copy_dependencies() {
    local lib=$1
    local output=$2

    # Run ldd to get the dependencies
    ldd "$lib" | while read -r line; do
        # Extract the path of each dependency
        dep=$(echo "$line" | awk '{print $3}')
        if [ -n "$dep" ] && [ -f "$dep" ] && [ ! -f "$output/$(basename "$dep")" ]; then
            echo "Copying: $dep"
            cp "$dep" "$output"
            # Recursively copy dependencies of the current dependency
            copy_dependencies "$dep" "$output"
        fi
    done
}

# Start the copying process
copy_dependencies "$LIB_PATH" "$OUTPUT_DIR"
