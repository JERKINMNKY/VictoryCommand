const fs = require('fs');
const path = require('path');

// Very small postprocessor to make quicktype C# output Unity-friendly:
// - Ensure using System; using System.Collections.Generic; using UnityEngine;
// - Add [System.Serializable] above public classes
// - Convert auto-properties to public fields (e.g., public string Id { get; set; } -> public string id;)

function read(file) { return fs.readFileSync(file, 'utf8'); }
function write(file, data) { fs.mkdirSync(path.dirname(file), { recursive: true }); fs.writeFileSync(file, data, 'utf8'); }

function postprocess(inputPath, outputPath) {
  let src = read(inputPath);

  // Add Unity using if missing
  if (!/using UnityEngine;/.test(src)) {
    src = src.replace(/(using System(?:\.Collections\.Generic)?;\s*)+/s, '$&using UnityEngine;\n');
  }

  // Insert [System.Serializable] before public class declarations
  src = src.replace(/public partial class ([A-Za-z0-9_]+)\s*\{/g, function(m, name) {
    return `[System.Serializable]\npublic partial class ${name} {`;
  });

  // Convert common auto-properties to public fields (simple heuristic)
  // from: public string Id { get; set; }
  // to:   public string id;
  src = src.replace(/public ([A-Za-z0-9_<>\[\]]+) ([A-Za-z0-9_]+) \{ get; set; \ }/g, function(m, type, prop) {
    // camelCase the prop name to match the schema field style
    const field = prop.charAt(0).toLowerCase() + prop.slice(1);
    return `public ${type} ${field};`;
  });

  // Write output
  write(outputPath, src);
}

if (require.main === module) {
  const input = process.argv[2];
  const output = process.argv[3];
  if (!input || !output) {
    console.error('Usage: node postprocess-cs.js <input> <output>');
    process.exit(2);
  }
  postprocess(input, output);
}
