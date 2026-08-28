#!/usr/bin/env bash
#
# Creates a new empty migration SQL file in PulseTech.Api/Migrations
# using the {timestamp}:{name}.sql format.
#
# Usage:
#   ./scripts/new-migration.sh add-users-table
#   ./scripts/new-migration.sh "add users table"   # spaces are converted to dashes

set -euo pipefail

# Separator between the timestamp and the name (e.g. ":" or "_")
SEP="${MIGRATION_SEP:-:}"

if [ "$#" -lt 1 ]; then
  echo "Error: provide a migration name." >&2
  echo "Example: $0 add-users-table" >&2
  exit 1
fi

# Join all arguments and normalize to kebab-case
raw="$*"
name="$(printf '%s' "$raw" \
  | tr '[:upper:]' '[:lower:]' \
  | tr -s ' _' '-' \
  | sed -E 's/[^a-z0-9-]//g; s/-+/-/g; s/^-+//; s/-+$//')"

if [ -z "$name" ]; then
  echo "Error: migration name is empty after normalization." >&2
  exit 1
fi

# Migrations directory relative to the script location
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
migrations_dir="$script_dir/../PulseTech.Api/Migrations"
mkdir -p "$migrations_dir"

timestamp="$(date +%Y%m%d%H%M%S)"
file="$migrations_dir/${timestamp}${SEP}${name}.sql"

if [ -e "$file" ]; then
  echo "Error: file already exists: $file" >&2
  exit 1
fi

cat > "$file" <<EOF
-- Migration: ${name}
-- Created: $(date +%Y-%m-%dT%H:%M:%S%z)

EOF

# Print the path relative to the repository when possible
rel="$(cd "$migrations_dir" && pwd)/${timestamp}${SEP}${name}.sql"
echo "Created: $rel"
