using System.Reflection;
using Sandbox = BlackBox.Machine.Sandbox;
using Path = System.IO.Path;

namespace System;

public static class System
{
	//todo: important: public static Cite(Path path)
	//should compile a c# file and then connect it to the process that called it so that its contents may be accessed with `using`
	//so something like
	//```System.Cite("User/MathLib.cs");
	//using MathLib;```
	//alternatively, we add a precompiler to executed scripts that searches for using statements with paths and pulls in the relevant file but that seems so open to jank
	
	private static string GetSimpleTypeName(Type type)
	{
		if (type == typeof(void)) return "void";
		if (type == typeof(int)) return "int";
		if (type == typeof(string)) return "string";
		if (type == typeof(bool)) return "bool";
		if (type == typeof(byte)) return "byte";
		if (type == typeof(char)) return "char";
		if (type == typeof(float)) return "float";
		if (type == typeof(double)) return "double";

		// Handle generic types
		if (type.IsGenericType)
		{
			var genericArgs = type.GetGenericArguments();
			var genericName = type.Name.Substring(0, type.Name.IndexOf('`'));
			var genericParams = string.Join(", ", genericArgs.Select(GetSimpleTypeName));
			return $"{genericName}<{genericParams}>";
		}

		return type.Name;
	}
	
	// show: "custom" = just user types, "system" = namespace list, "all" = all classes
	public static void Help(string className = "", string show = "custom") //todo: fix getting from excluded system assemblies in some cases
	{
		if (className == "")
		{
			// Get all assemblies available in the sandbox
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
				.ToList();

			// Get all public types
			var allTypes = assemblies
				.SelectMany(a => {
					try { return a.GetTypes(); }
					catch { return Array.Empty<Type>(); }
				})
				.Where(t => t.Namespace != null && t.IsPublic);

			if (show == "namespace")
			{
				// Show only namespaces for system types
				var namespaces = allTypes
					.Where(t => t.Namespace.StartsWith("System"))
					.Select(t => t.Namespace)
					.Distinct()
					.OrderBy(ns => ns);

				Terminal.Write("Available System namespaces:\n");
				foreach (var ns in namespaces)
				{
					Terminal.Write($"- {ns}\n");
				}
			}
			else
			{
				// Show individual types
				IEnumerable<Type> systemTypes = show switch
				{
					"simple" => allTypes.Where(t =>
						t.Assembly == typeof(System).Assembly && t.Namespace.StartsWith("System")),
					"all" => allTypes.Where(t => t.Namespace.StartsWith("System")),
					_ => allTypes.Where(t =>
						t.Assembly == typeof(System).Assembly && t.Namespace.StartsWith("System"))
				};

				systemTypes = systemTypes.OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToList();

				string currentNamespace = "";
				foreach (var t in systemTypes)
				{
					if (t.Namespace != currentNamespace)
					{
						currentNamespace = t.Namespace!;
						Terminal.Write($"{currentNamespace}:\n");
					}
					Terminal.Write($"- {t.Name}\n");
				}
			}
		}
		else
		{
			// Search in all assemblies for the specific type
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
				.ToList();
/*
			var type = assemblies
				.SelectMany(a => {
					try { return a.GetTypes(); }
					catch { return Array.Empty<Type>(); }
				})
				.FirstOrDefault(t => t.Namespace != null && t.Namespace.StartsWith("System") && !t.Namespace.StartsWith("System.IO") && t.Name == className && t.IsPublic);
*/
			var type = typeof(Path).Assembly.GetTypes().FirstOrDefault( t=> t.Namespace != null && t.Namespace.StartsWith("System") && t.Name == className && t.IsPublic);

			if (type == null)
			{
				Terminal.Write($"Class '{className}' not found\n");
				return;
			}

			Terminal.Write($"{type.Name}:\n");

			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(m => !m.IsSpecialName)
				.OrderBy(m => m.Name)
				.ToList();

			if (methods.Count == 0)
			{
				Terminal.Write($"- {type.Name} has no methods\n");
			}
			
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly )
				.OrderBy(p => p.Name)
				.ToList();
			
			if (properties.Count == 0)
			{
				Terminal.Write($"- {type.Name} has no properties\n");
			}

			foreach (var prop in properties)
			{
				var propType = GetSimpleTypeName(prop.PropertyType);
				Terminal.Write($"- {prop.Name}: {propType}\n");
			}

			foreach (var method in methods)
			{
				var parameters = string.Join(", ", method.GetParameters().Select(p =>
					$"{GetSimpleTypeName(p.ParameterType)} {p.Name}"));
				var returnType = GetSimpleTypeName(method.ReturnType);
				Terminal.Write($"- {method.Name}({parameters}): {returnType}\n");
			}
		}
	}
	
	public static void Vars()
	{
		var vars = Sandbox.GetVariables().ToList();
		if (vars.Count == 0)
		{
			Terminal.Write("No variables defined\n");
		}
		else
		{
			Terminal.Write("Environment Variables:\n");
			foreach (var v in vars)
			{
				Terminal.Write($"  {v.Name} ({v.Type.Name}) = {v.Value}\n");
			}
		}
	}

	public static void Execute(string path)
	{
		var result = Sandbox.Execute(new Path(path).Read());

		if (result.Success)
		{
			if (result.ReturnValue != null)
			{
				Terminal.WriteLine($"=> {result.ReturnValue}");
			}
		}
		else
		{
			Console.Error.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
			Terminal.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
		}
	}
	
	public static void List(string path)
	{
		//list files
	}

	public static void Touch(string path)
	{
		//initialize file
	}
	
	//Move()
	//Copy()
	
}