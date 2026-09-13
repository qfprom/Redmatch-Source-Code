#!/bin/bash

echo "Give the path to the Incorrect guids (the package folder containing them) usually '/Assets/Scripts/[Package]'"
read -r incorrect_path
incorrect_path="${incorrect_path%\'}"
incorrect_path="${incorrect_path#\'}"

echo "Give the path where the actual package is that usually being '/Library/PackageCache/[Package]'"
read -r correct_path
correct_path="${correct_path%\'}"
correct_path="${correct_path#\'}"

python3 "$(dirname "$0")/guid_fixer.py" "$incorrect_path" "$correct_path"
