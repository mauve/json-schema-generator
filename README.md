# README.md

A simple commandline wrapper around `NJsonSchema` to generate C# classes from a JSON schema.

## How to use

```sh
dotnet run -- --input schema/CVE_Record_Format.json --namespace CveModels --generateOptionalPropertiesAsNullable true --generateDataAnnotations true --useRequiredKeyword true --jsonLibrary SystemTextJson
```