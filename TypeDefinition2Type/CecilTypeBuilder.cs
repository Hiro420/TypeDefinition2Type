using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;

namespace TypeDefinition2Type;

public class CecilTypeBuilder
{
	private readonly ModuleBuilder _moduleBuilder;
	private readonly Dictionary<string, Type> _builtTypes = new();

	public CecilTypeBuilder(string assemblyName = "DynamicCecilAssembly")
	{
		AssemblyName asmName = new AssemblyName(assemblyName);
		AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
		_moduleBuilder = asmBuilder.DefineDynamicModule(assemblyName);
	}

	public Type BuildType(TypeDefinition typeDef)
	{
		if (_builtTypes.TryGetValue(typeDef.FullName, out Type? cachedType))
			return cachedType;

		TypeBuilder typeBuilder = _moduleBuilder.DefineType(typeDef.Name,
			System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Class);

		_builtTypes[typeDef.FullName] = typeBuilder;

		foreach (FieldDefinition field in typeDef.Fields)
		{
			Type fieldType = ResolveTypeReference(field.FieldType);
			typeBuilder.DefineField(field.Name, fieldType, System.Reflection.FieldAttributes.Public);
		}

		foreach (PropertyDefinition prop in typeDef.Properties)
		{
			Type propType = ResolveTypeReference(prop.PropertyType);
			FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{prop.Name}", propType, System.Reflection.FieldAttributes.Private);

			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prop.Name, System.Reflection.PropertyAttributes.None, propType, null);

			// Getter
			if (prop.GetMethod != null)
			{
				MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(prop.GetMethod.Name,
				System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.HideBySig,
				propType, Type.EmptyTypes);

				ILGenerator getIL = getMethodBuilder.GetILGenerator();
				getIL.Emit(OpCodes.Ldarg_0);
				getIL.Emit(OpCodes.Ldfld, fieldBuilder);
				getIL.Emit(OpCodes.Ret);
				propertyBuilder.SetGetMethod(getMethodBuilder);
			}

			// Setter
			if (prop.SetMethod != null)
			{
				MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(prop.SetMethod.Name,
				System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.HideBySig,
				null, new[] { propType });

				ILGenerator setIL = setMethodBuilder.GetILGenerator();
				setIL.Emit(OpCodes.Ldarg_0);
				setIL.Emit(OpCodes.Ldarg_1);
				setIL.Emit(OpCodes.Stfld, fieldBuilder);
				setIL.Emit(OpCodes.Ret);
				propertyBuilder.SetSetMethod(setMethodBuilder);
			}
		}

		// TODO: CustomAttributes, more methods etc
		return typeBuilder.CreateType();
	}

	private Type ResolveTypeReference(TypeReference typeRef)
	{
		if (typeRef == null)
			throw new ArgumentNullException(nameof(typeRef));

		if (typeRef is ArrayType arrayType)
		{
			Type elementType = ResolveTypeReference(arrayType.ElementType);
			return elementType.MakeArrayType(arrayType.Rank);
		}

		if (typeRef.FullName == "System.String") return typeof(string);
		if (typeRef.FullName == "System.Int32") return typeof(int);
		if (typeRef.FullName == "System.Boolean") return typeof(bool);
		if (typeRef.FullName == "System.Byte") return typeof(byte);
		if (typeRef.FullName == "System.Void") return typeof(void);
		if (typeRef.FullName == "System.Object") return typeof(object);
		if (typeRef.FullName == "System.Single") return typeof(float);
		if (typeRef.FullName == "System.Double") return typeof(double);
		if (typeRef.FullName == "System.Int64") return typeof(long);
		if (typeRef.FullName == "System.UInt32") return typeof(uint);
		if (typeRef.FullName == "System.UInt64") return typeof(ulong);
		if (typeRef.FullName == "System.Char") return typeof(char);

		if (typeRef is TypeDefinition td)
			return BuildType(td);

		TypeDefinition? resolved = typeRef.Resolve();
		if (resolved != null)
			return BuildType(resolved);

		try
		{
			return Type.GetType(typeRef.FullName) ?? throw new NotSupportedException();
		}
		catch
		{
			throw new NotSupportedException($"Cannot resolve type: {typeRef.FullName}");
		}
	}

}
