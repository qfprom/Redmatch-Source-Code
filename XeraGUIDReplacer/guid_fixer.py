#!/usr/bin/env python3

import os
import sys
from pathlib import Path
from collections import defaultdict

def read_guid_from_meta(meta_path):
    """Extract GUID from a .meta file."""
    try:
        with open(meta_path, 'r', encoding='utf-8', errors='ignore') as f:
            for line in f:
                if 'guid:' in line:
                    pos = line.find('guid:') + 6
                    return line[pos:pos+32].strip()
    except Exception as e:
        print(f"Error reading {meta_path}: {e}")
    return ""

def build_guid_map(incorrect_path, correct_path):
    """Build a mapping of incorrect -> correct GUIDs."""
    guid_map = {}
    incorrect_meta_files = {}

    print("Building GUID map...")

    # Collect incorrect GUIDs
    incorrect_p = Path(incorrect_path)
    for meta_file in incorrect_p.rglob("*.meta"):
        guid = read_guid_from_meta(meta_file)
        if guid:
            incorrect_meta_files[meta_file.name] = guid

    # Match with correct GUIDs
    correct_p = Path(correct_path)
    for meta_file in correct_p.rglob("*.meta"):
        filename = meta_file.name
        if filename in incorrect_meta_files:
            correct_guid = read_guid_from_meta(meta_file)
            if correct_guid:
                incorrect_guid = incorrect_meta_files[filename]
                guid_map[incorrect_guid] = correct_guid
                print(f"Mapped: {incorrect_guid} -> {correct_guid}")

    return guid_map

def process_project_files(project_path, guid_map):
    """Replace incorrect GUIDs with correct ones in all project files."""
    if not guid_map:
        print("No GUID mappings found!")
        return 0

    print("\nProcessing Unity project files...")
    files_modified = 0
    project_p = Path(project_path)

    for file_path in project_p.rglob("*"):
        if not file_path.is_file():
            continue

        try:
            with open(file_path, 'rb') as f:
                content = f.read()

            modified = False
            content_str = content.decode('utf-8', errors='ignore')

            # Replace all incorrect GUIDs
            for incorrect_guid, correct_guid in guid_map.items():
                if incorrect_guid in content_str:
                    content_str = content_str.replace(incorrect_guid, correct_guid)
                    modified = True

            if modified:
                with open(file_path, 'w', encoding='utf-8', errors='ignore') as f:
                    f.write(content_str)
                print(f"Corrected: {file_path.name}")
                files_modified += 1

        except Exception as e:
            print(f"Error processing {file_path}: {e}")

    return files_modified

def main():
    project_path = str(Path(__file__).parent / "Assets")

    if len(sys.argv) == 3:
        incorrect_path = sys.argv[1]
        correct_path = sys.argv[2]
    else:
        print("Give the path to the Incorrect guids (the package folder containing them) usually '/Assets/Scripts/[Package]'")
        incorrect_path = input().strip()

        print("Give the path where the actual package is that usually being '/Library/PackageCache/[Package]'")
        correct_path = input().strip()

    print(f"Using project path: {project_path}")

    # Validate paths
    if not all(Path(p).exists() for p in [incorrect_path, correct_path, project_path]):
        print("ERROR: One or more paths don't exist! Double check and try again")
        return 1

    # Build GUID map
    guid_map = build_guid_map(incorrect_path, correct_path)

    if not guid_map:
        print("No GUID mappings found!")
        return 1

    # Process files
    files_modified = process_project_files(project_path, guid_map)

    print(f"\nDone! Modified {files_modified} files.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
