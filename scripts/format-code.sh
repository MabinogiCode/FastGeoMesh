#!/bin/bash
# FastGeoMesh Code Formatting Script
# For local development and pre-commit hooks on Unix systems

set -euo pipefail

# Default values
FIX=false
CHECK=false
CONFIGURATION="Release"

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
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --help|-h)
            echo "?? FastGeoMesh Code Formatting Tool"
            echo ""
            echo "Usage:"
            echo "  $0 --fix     # Fix formatting issues"
            echo "  $0 --check   # Check formatting only"
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

echo "?? FastGeoMesh Code Formatting Tool"

# Ensure we're in the solution directory
if ! ls *.sln >/dev/null 2>&1; then
    echo "? No solution file found. Please run from solution directory."
    exit 1
fi

SOLUTION_FILE=$(ls *.sln | head -n 1)
echo "?? Working with solution: $SOLUTION_FILE"

format_code() {
    echo "?? Running code formatter (fixing issues)..."
    
    # Save current git status
    GIT_STATUS_BEFORE=$(git status --porcelain 2>/dev/null || echo "")
    
    # Run formatter
    if dotnet format --verbosity normal; then
        echo "? Code formatting completed successfully"
        
        # Check git status after formatting
        GIT_STATUS_AFTER=$(git status --porcelain 2>/dev/null || echo "")
        
        if [[ "$GIT_STATUS_BEFORE" != "$GIT_STATUS_AFTER" ]]; then
            CHANGED_FILES=$(git diff --name-only 2>/dev/null || echo "")
            if [[ -n "$CHANGED_FILES" ]]; then
                echo "?? Formatted files:"
                echo "$CHANGED_FILES" | sed 's/^/  - /'
                echo "?? Files have been auto-formatted. Please review and commit changes."
            else
                echo "? No changes detected after formatting."
            fi
        else
            echo "? No formatting changes needed."
        fi
    else
        echo "? Code formatting failed"
        exit 1
    fi
}

check_formatting() {
    echo "?? Checking code formatting (no changes)..."
    
    if dotnet format --verify-no-changes --verbosity normal; then
        echo "? Code is properly formatted"
    else
        echo "??  Code formatting issues detected"
        echo "?? Run '$0 --fix' to auto-fix formatting"
        exit 1
    fi
}

show_status() {
    echo ""
    echo "Usage:"
    echo "  $0 --fix     # Fix formatting issues"
    echo "  $0 --check   # Check formatting only"
    echo ""
    
    echo "?? Quick formatting status check..."
    
    # Save current state
    GIT_STATUS_BEFORE=$(git status --porcelain 2>/dev/null || echo "")
    
    # Run quick format check
    if dotnet format --verify-no-changes --verbosity quiet >/dev/null 2>&1; then
        echo "? Code is properly formatted"
    else
        echo "??  Formatting issues detected. Use --fix to resolve."
    fi
    
    # Ensure no changes were made during check
    GIT_STATUS_AFTER=$(git status --porcelain 2>/dev/null || echo "")
    if [[ "$GIT_STATUS_BEFORE" != "$GIT_STATUS_AFTER" ]]; then
        echo "??  Unexpected: Check mode modified files. Reverting..."
        git checkout . 2>/dev/null || true
    fi
    
    echo ""
    echo "?? Tip: Add this to your pre-commit hook:"
    echo "    $0 --check"
}

# Main logic
if [[ "$FIX" == "true" ]]; then
    format_code
elif [[ "$CHECK" == "true" ]]; then
    check_formatting
else
    show_status
fi

echo ""
echo "?? Formatting operations completed"