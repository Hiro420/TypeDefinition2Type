# TypeDefinition2Type
Convert Mono.Cecil TypeDefinition to Reflection.Type, made for Il2CppDumper's DummyDLL output

# Usage
- Compile via Visual Studio 2022
- `TypeDefinition2Type.exe <DummyDLL file path> <TypeDefinition FullName>`

## This is just a proof-of-concept tool and isn't intended to be a final product. Feel free to modify it to your needs. It's very minimal on purpose, and is meant to be embedded into your code.

# Why?
Because I'm sick of Mono.Cecil TypeDefinition system since you can't create instances of the C# types, so I created this tool to help me with it.