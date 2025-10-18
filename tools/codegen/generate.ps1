Param(
    [string]$Out = "all"
)

Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Definition)
Push-Location -Path ..\..

if ($Out -eq 'ts') {
    npx quicktype --lang typescript --src docs/specs/game_spec.full.schema.json -o docs/specs/models.ts --just-types
} elseif ($Out -eq 'csharp') {
    npx quicktype --lang cs --namespace VictoryCommand.Schema --src docs/specs/game_spec.full.schema.json -o Assets/Scripts/Generated/GameModels.Generated.cs
} else {
    npx quicktype --lang typescript --src docs/specs/game_spec.full.schema.json -o docs/specs/models.ts --just-types
    npx quicktype --lang cs --namespace VictoryCommand.Schema --src docs/specs/game_spec.full.schema.json -o Assets/Scripts/Generated/GameModels.Generated.cs
}

Pop-Location
Pop-Location
