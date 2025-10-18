using System;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

class Program {
    static async Task<int> Main(string[] args) {
        var schemaPath = "../../docs/specs/game_spec.full.schema.json";
        var outputPath = "../../Assets/Scripts/Generated/GameModels.NJson.cs";
        var ns = "VictoryCommand.Schema";

        // simple arg parsing
        for (int i=0;i<args.Length;i++) {
            if (args[i]=="--schemaPath" && i+1<args.Length) schemaPath = args[++i];
            if (args[i]=="--output" && i+1<args.Length) outputPath = args[++i];
            if (args[i]=="--namespace" && i+1<args.Length) ns = args[++i];
        }

        if (!File.Exists(schemaPath)) {
            Console.Error.WriteLine($"Schema not found: {schemaPath}");
            return 2;
        }

        var schema = await JsonSchema.FromFileAsync(schemaPath);

        var settings = new CSharpGeneratorSettings {
            Namespace = ns,
            GenerateAbstractProperties = false,
            ClassStyle = CSharpClassStyle.Poco, // produce POCOs
            GenerateDataAnnotations = false
        };

        var generator = new CSharpGenerator(schema, settings);
        var code = generator.GenerateFile();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, code);
        Console.WriteLine($"Wrote: {outputPath}");
        return 0;
    }
}
