using System.Reflection;
using Mono.Cecil;

namespace TypeDefinition2Type;

internal class MainApp
{
	public static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine($"Usage: TypeDefinition2Type.exe <DummyDLL file path> <TypeDefinition FullName>");
		}

		string DummyDLLPath = args[0]; // like "C:\Users\User\Downloads\SomeGameOutput\DummyDLL"
		string TypeDefName = args[1]; // like "UnityEngine.Rigidbody2D"

		if (!File.Exists(DummyDLLPath))
		{
			Console.WriteLine($"Error: The file at '{DummyDLLPath}' does not exist.");
			return;
		}

		DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
		resolver.AddSearchDirectory(Directory.GetParent(DummyDLLPath)?.FullName ?? string.Empty);
		ReaderParameters readerParams = new ReaderParameters { AssemblyResolver = resolver };
		ModuleDefinition metaData = AssemblyDefinition.ReadAssembly(DummyDLLPath, readerParams).MainModule;

		TypeDefinition? typeDef = metaData.Types.FirstOrDefault(t => t.FullName == TypeDefName);

		if (typeDef == null)
		{
			Console.WriteLine($"Could not find a type of full name {TypeDefName} in {DummyDLLPath}");
			return;
		}

		CecilTypeBuilder builder = new CecilTypeBuilder();
		Type generatedType = builder.BuildType(typeDef);

		Console.WriteLine($"Generated type: {generatedType.FullName}");

		object instance = Activator.CreateInstance(generatedType)!;
		Console.WriteLine($"Instance created: {instance}");

		Console.WriteLine("\nFields:");	
		foreach (FieldInfo fieldInfo in instance.GetType().GetFields())
		{
			Console.WriteLine(fieldInfo);	
		}

		Console.WriteLine("\nProperties:");
		foreach (PropertyInfo propertyInfo in instance.GetType().GetProperties())
		{
			Console.WriteLine(propertyInfo);
		}

		Console.WriteLine("\nMethods:");
		foreach (MethodInfo methodInfo in instance.GetType().GetMethods())
		{
			Console.WriteLine(methodInfo);
		}
	}
}
