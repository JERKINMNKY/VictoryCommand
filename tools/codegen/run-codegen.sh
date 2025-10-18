#!/usr/bin/env bash
set -euo pipefail

# Run quicktype (Node) in a container and NJsonSchema generator (dotnet) in a container
# Usage: ./run-codegen.sh

REPO_ROOT="$(pwd)"

echo "Running quicktype generation in node container..."
docker run --rm -v "$REPO_ROOT:/work" -w /work node:20 bash -lc "cd tools/codegen && npm ci && npm run generate:all"

echo "Running NJsonSchema generator in dotnet sdk container..."
docker run --rm -v "$REPO_ROOT:/work" -w /work mcr.microsoft.com/dotnet/sdk:8.0 bash -lc "cd tools/codegen/njsonschema-gen && dotnet restore && dotnet run -- --schemaPath ../..//docs/specs/game_spec.full.schema.json --output ../../Assets/Scripts/Generated/GameModels.NJson.cs --namespace VictoryCommand.Schema"

echo "Code generation complete."
