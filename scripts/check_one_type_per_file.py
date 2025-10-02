#!/usr/bin/env python3
import os
import re
import sys

# Regex pour identifier class / struct / enum
DECLARATION_RE = re.compile(r'^\s*(public|internal|protected|private)?\s*(partial\s+)?(class|struct|enum)\s+\w+')

def check_file(path):
    count = 0
    with open(path, encoding="utf-8") as f:
        for line in f:
            # Compte chaque déclaration de type
            if DECLARATION_RE.match(line):
                count += 1
    return count

def main(root="."):
    errors = []
    for dirpath, _, filenames in os.walk(root):
        for fname in filenames:
            if fname.endswith(".cs"):
                fpath = os.path.join(dirpath, fname)
                count = check_file(fpath)
                if count > 1:
                    errors.append((fpath, count))
    if errors:
        print("❌ Files with more than one class/struct/enum:")
        for path, count in errors:
            print(f"  {path}: {count} top-level types")
        sys.exit(1)  # ❌ échoue la CI
    else:
        print("✅ All .cs files contain at most one class/struct/enum.")
        sys.exit(0)

if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else ".")
