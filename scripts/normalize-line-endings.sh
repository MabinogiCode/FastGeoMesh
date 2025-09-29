#!/bin/bash
# 🔧 FastGeoMesh Line Endings Normalization Script
# Ensures consistent line endings across the entire codebase

set -euo pipefail

# Default values
FIX=false
CHECK=false
TARGET_FORMAT="LF"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --fix|-f)
            FIX=true
            shift
            ;;
        --check|-c)
            CHECK=true
            shift
            ;;
        --target)
            TARGET_FORMAT="$2"
            shift 2
            ;;
        --help|-h)
            echo "🔧 FastGeoMesh Line Endings Normalization"
            echo ""
            echo "Usage:"
            echo "  $0 --fix     # Fix line endings to LF"
            echo "  $0 --check   # Check for line ending issues"
            echo "  $0 --help    # Show this help"
            echo ""
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "🔧 FastGeoMesh Line Endings Normalization"

# File patterns to normalize
patterns=(
    "*.cs" "*.csproj" "*.sln" "*.md" "*.yml" "*.yaml"
    "*.json" "*.xml" "*.txt" "*.sh" "*.ps1" "*.config"
    "*.props" "*.targets" "*.runsettings"
)

# Directories to process
directories=("src" "tests" "samples" "scripts" ".github" "docs")

get_line_ending_type() {
    local file="$1"
    
    if [[ ! -f "$file" ]]; then
        echo "Error"
        return
    fi
    
    # Check for CRLF
    if grep -q $'\r\n' "$file" 2>/dev/null; then
        echo "CRLF"
        return
    fi
    
    # Check for CR only
    if grep -q $'\r' "$file" 2>/dev/null; then
        echo "CR"
        return
    fi
    
    # Check for LF
    if grep -q $'\n' "$file" 2>/dev/null; then
        echo "LF"
        return
    fi
    
    echo "None"
}

convert_line_endings() {
    local file="$1"
    local target="$2"
    
    # Create backup
    cp "$file" "$file.bak"
    
    case "$target" in
        "LF")
            # Convert to LF
            sed -i 's/\r$//' "$file" 2>/dev/null || {
                # Fallback for systems without GNU sed
                tr -d '\r' < "$file.bak" > "$file"
            }
            ;;
        "CRLF")
            # Convert to CRLF
            sed -i 's/$/\r/' "$file" 2>/dev/null || {
                # Fallback
                sed 's/$/\r/' "$file.bak" > "$file"
            }
            ;;
    esac
    
    # Remove backup if successful
    if [[ $? -eq 0 ]]; then
        rm "$file.bak"
        return 0
    else
        # Restore backup on failure
        mv "$file.bak" "$file"
        return 1
    fi
}

# Find all files to process
all_files=()

for dir in "${directories[@]}"; do
    if [[ -d "$dir" ]]; then
        for pattern in "${patterns[@]}"; do
            while IFS= read -r -d '' file; do
                all_files+=("$file")
            done < <(find "$dir" -name "$pattern" -type f -print0 2>/dev/null)
        done
    fi
done

# Add root level files
for pattern in "${patterns[@]}"; do
    while IFS= read -r -d '' file; do
        all_files+=("$file")
    done < <(find . -maxdepth 1 -name "$pattern" -type f -print0 2>/dev/null)
done

# Remove duplicates and sort
IFS=$'\n' all_files=($(printf '%s\n' "${all_files[@]}" | sort -u))

echo "📁 Found ${#all_files[@]} files to process"

if [[ "$CHECK" == "true" ]]; then
    echo "🔍 Checking line endings..."
    
    issues=0
    for file in "${all_files[@]}"; do
        line_ending=$(get_line_ending_type "$file")
        if [[ "$line_ending" != "$TARGET_FORMAT" && "$line_ending" != "None" && "$line_ending" != "Error" ]]; then
            echo "❌ $file: $line_ending (expected: $TARGET_FORMAT)"
            ((issues++))
        fi
    done
    
    if [[ $issues -eq 0 ]]; then
        echo "✅ All files have correct line endings ($TARGET_FORMAT)"
    else
        echo "❌ Found $issues files with incorrect line endings"
        echo "💡 Run '$0 --fix' to correct these issues"
        exit 1
    fi
    
elif [[ "$FIX" == "true" ]]; then
    echo "🔧 Fixing line endings to $TARGET_FORMAT..."
    
    fixed=0
    errors=0
    
    for file in "${all_files[@]}"; do
        line_ending=$(get_line_ending_type "$file")
        if [[ "$line_ending" != "$TARGET_FORMAT" && "$line_ending" != "None" && "$line_ending" != "Error" ]]; then
            echo "  📝 $(basename "$file") ($line_ending -> $TARGET_FORMAT)"
            
            if convert_line_endings "$file" "$TARGET_FORMAT"; then
                ((fixed++))
            else
                ((errors++))
                echo "    ❌ Failed to convert $file"
            fi
        fi
    done
    
    echo "✅ Fixed $fixed files"
    if [[ $errors -gt 0 ]]; then
        echo "❌ Failed to fix $errors files"
        exit 1
    fi
    
else
    # Show status
    echo "📊 Line endings analysis:"
    
    declare -A stats
    for file in "${all_files[@]}"; do
        line_ending=$(get_line_ending_type "$file")
        ((stats["$line_ending"]++))
    done
    
    for key in "${!stats[@]}"; do
        case "$key" in
            "$TARGET_FORMAT") echo "  $key: ${stats[$key]} files ✅" ;;
            "Mixed"|"Error") echo "  $key: ${stats[$key]} files ❌" ;;
            *) echo "  $key: ${stats[$key]} files ⚠️" ;;
        esac
    done
    
    echo ""
    echo "Usage:"
    echo "  $0 --check  # Check for issues"
    echo "  $0 --fix    # Fix line endings"
fi

echo ""
echo "🎯 Line ending normalization completed"
