const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

function run(cmd) {
  console.log('> ' + cmd);
  execSync(cmd, { stdio: 'inherit' });
}

function main() {
  const root = path.resolve(__dirname, '..', '..');
  process.chdir(root);

  // regenerate into a temp dir
  const tmpDir = path.join(root, 'tmp_codegen');
  if (fs.existsSync(tmpDir)) fs.rmSync(tmpDir, { recursive: true, force: true });
  fs.mkdirSync(tmpDir);

  // run quicktype via npx to generate into tmp files
  run('npx quicktype --lang typescript --src docs/specs/game_spec.full.schema.json -o ' + path.join(tmpDir, 'models.ts') + ' --just-types');
  run('npx quicktype --lang cs --namespace VictoryCommand.Schema --src docs/specs/game_spec.full.schema.json -o ' + path.join(tmpDir, 'GameModels.Generated.cs'));
  // postprocess the C# into Unity-friendly form in the tmp dir
  run('node postprocess-cs.js ' + path.join(tmpDir, 'GameModels.Generated.cs') + ' ' + path.join(tmpDir, 'GameModels.Generated.post.cs'));

  // compare the generated files with committed ones
  const checks = [
    { committed: path.join(root, 'docs/specs/models.ts'), generated: path.join(tmpDir, 'models.ts') },
  { committed: path.join(root, 'Assets/Scripts/Generated/GameModels.Generated.cs'), generated: path.join(tmpDir, 'GameModels.Generated.post.cs') }
  ];

  let changed = false;
  for (const c of checks) {
    const a = fs.existsSync(c.committed) ? fs.readFileSync(c.committed, 'utf8') : '';
    const b = fs.readFileSync(c.generated, 'utf8');
    if (a !== b) {
      console.error('Generated file differs: ' + c.committed);
      changed = true;
    }
  }

  if (changed) {
    console.error('\nGenerated artifacts are out of date. Run `npm run gen:all` in tools/codegen to update them.');
    process.exit(2);
  } else {
    console.log('Generated artifacts are up to date.');
  }
}

main();
