using System.CommandLine;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace gen;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var inputOption = new Option<string>(
            "--input",
            "The JSON schema file to generate C# classes from."
        )
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne,
        };
        var namespaceOption = new Option<string>(
            "--namespace",
            "The namespace for the generated classes"
        );
        namespaceOption.SetDefaultValue("GeneratedNamespace");
        var generateDataAnnotationsOption = new Option<bool>(
            "--generateDataAnnotations",
            "Generate data annotations"
        );
        var generateJsonMethodsOption = new Option<bool>(
            "--generateJsonMethods",
            "Generate JSON methods"
        );
        var generateImmutableArrayPropertiesOptions = new Option<bool>(
            "--generateImmutableArrayProperties",
            "Generate immutable array properties"
        );
        var generateImmutableDictionaryPropertiesOption = new Option<bool>(
            "--generateImmutableDictionaryProperties",
            "Generate immutable dictionary properties"
        );
        var generateDefaultValuesOption = new Option<bool>(
            "--generateDefaultValues",
            "Generate default values"
        );
        var generateOptionalPropertiesAsNullableOption = new Option<bool>(
            "--generateOptionalPropertiesAsNullable",
            "Generate optional properties as nullable"
        );
        var jsonLibraryOption = new Option<string>(
            "--jsonLibrary",
            "The JSON library to use (NewtonsoftJson or SystemTextJson)"
        )
        {
            IsRequired = false,
            Arity = ArgumentArity.ZeroOrOne,
        };
        jsonLibraryOption.SetDefaultValue("SystemTextJson");
        var useRequiredKeywordOption = new Option<bool>(
            "--useRequiredKeyword",
            "Use the C# 11 'required' keyword for required properties"
        )
        {
            IsRequired = false,
            Arity = ArgumentArity.ZeroOrOne,
        };
        useRequiredKeywordOption.SetDefaultValue(true);
        var enforceFlagEnumsOption = new Option<bool>("--enforceFlagEnums", "Enforce flag enums")
        {
            IsRequired = false,
            Arity = ArgumentArity.ZeroOrOne,
        };

        var rootCommand = new RootCommand("Generate C# classes from a JSON Schema")
        {
            inputOption,
            namespaceOption,
            generateDataAnnotationsOption,
            generateJsonMethodsOption,
            generateImmutableArrayPropertiesOptions,
            generateImmutableDictionaryPropertiesOption,
            generateDefaultValuesOption,
            generateOptionalPropertiesAsNullableOption,
            jsonLibraryOption,
            useRequiredKeywordOption,
            enforceFlagEnumsOption,
        };

        rootCommand.Description = "Generate C# classes from JSON schema files";

        rootCommand.SetHandler(async context =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption);
            var @namespace = context.ParseResult.GetValueForOption(namespaceOption);
            var generateDataAnnotations = context.ParseResult.GetValueForOption(
                generateDataAnnotationsOption
            );
            var generateJsonMethods = context.ParseResult.GetValueForOption(
                generateJsonMethodsOption
            );
            var generateImmutableArrayProperties = context.ParseResult.GetValueForOption(
                generateImmutableArrayPropertiesOptions
            );
            var generateImmutableDictionaryProperties = context.ParseResult.GetValueForOption(
                generateImmutableDictionaryPropertiesOption
            );
            var generateDefaultValues = context.ParseResult.GetValueForOption(
                generateDefaultValuesOption
            );
            var generateOptionalPropertiesAsNullable = context.ParseResult.GetValueForOption(
                generateOptionalPropertiesAsNullableOption
            );
            var jsonLibrary = context.ParseResult.GetValueForOption(jsonLibraryOption);
            var useRequiredKeyword = context.ParseResult.GetValueForOption(
                useRequiredKeywordOption
            );
            var enforceFlagEnums = context.ParseResult.GetValueForOption(enforceFlagEnumsOption);

            async Task<int> HandlerImpl()
            {
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Inmut is required.");
                    return 1;
                }

                if (!File.Exists(input))
                {
                    Console.WriteLine($"Input '{input}' does not exist.");
                    return 1;
                }

                try
                {
                    var schema = await JsonSchema.FromFileAsync(input);

                    var settings = new CSharpGeneratorSettings
                    {
                        Namespace = @namespace!,
                        GenerateDataAnnotations = generateDataAnnotations,
                        GenerateJsonMethods = generateJsonMethods,
                        GenerateImmutableArrayProperties = generateImmutableArrayProperties,
                        GenerateImmutableDictionaryProperties =
                            generateImmutableDictionaryProperties,
                        GenerateDefaultValues = generateDefaultValues,
                        GenerateOptionalPropertiesAsNullable = generateOptionalPropertiesAsNullable,
                        JsonLibrary = jsonLibrary switch
                        {
                            "NewtonsoftJson" => CSharpJsonLibrary.NewtonsoftJson,
                            "SystemTextJson" => CSharpJsonLibrary.SystemTextJson,
                            _ => throw new ArgumentException(
                                "Invalid JSON library specified. Use 'NewtonsoftJson' or 'SystemTextJson'."
                            ),
                        },
                        UseRequiredKeyword = useRequiredKeyword,
                        EnforceFlagEnums = enforceFlagEnums,
                    };

                    var generator = new CSharpGenerator(schema, settings);
                    var codeFile = generator.GenerateFile();

                    var outputFileName = Path.Combine(
                        Path.GetDirectoryName(input) ?? string.Empty,
                        Path.GetFileNameWithoutExtension(input) + ".cs"
                    );
                    await File.WriteAllTextAsync(outputFileName, codeFile);

                    Console.WriteLine($"Generated C# classes for schema: {input}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing schema file '{input}': {ex.Message}");
                }

                return 0;
            }

            context.ExitCode = await HandlerImpl();
        });

        return await rootCommand.InvokeAsync(args);
    }
}
