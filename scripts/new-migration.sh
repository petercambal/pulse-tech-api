#!/usr/bin/env bash
#
# Vytvori novy prazdny migracny SQL subor v PulseTech.Api/Migrations
# vo formate {timestamp}:{nazov}.sql
#
# Pouzitie:
#   ./scripts/new-migration.sh add-users-table
#   ./scripts/new-migration.sh "add users table"   # medzery sa prevedu na pomlcky

set -euo pipefail

# Oddelovac medzi timestampom a nazvom (napr. ":" alebo "_")
SEP="${MIGRATION_SEP:-:}"

if [ "$#" -lt 1 ]; then
  echo "Chyba: zadaj nazov migracie." >&2
  echo "Priklad: $0 add-users-table" >&2
  exit 1
fi

# Spoj vsetky argumenty a normalizuj na kebab-case
raw="$*"
name="$(printf '%s' "$raw" \
  | tr '[:upper:]' '[:lower:]' \
  | tr -s ' _' '-' \
  | sed -E 's/[^a-z0-9-]//g; s/-+/-/g; s/^-+//; s/-+$//')"

if [ -z "$name" ]; then
  echo "Chyba: nazov migracie je po normalizacii prazdny." >&2
  exit 1
fi

# Adresar Migrations relativne k umiestneniu skriptu
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
migrations_dir="$script_dir/../PulseTech.Api/Migrations"
mkdir -p "$migrations_dir"

timestamp="$(date +%Y%m%d%H%M%S)"
file="$migrations_dir/${timestamp}${SEP}${name}.sql"

if [ -e "$file" ]; then
  echo "Chyba: subor uz existuje: $file" >&2
  exit 1
fi

cat > "$file" <<EOF
-- Migration: ${name}
-- Created: $(date +%Y-%m-%dT%H:%M:%S%z)

EOF

# Vypis cestu relativnu k repozitaru ak sa da
rel="$(cd "$migrations_dir" && pwd)/${timestamp}${SEP}${name}.sql"
echo "Vytvorene: $rel"
