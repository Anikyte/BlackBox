using Microsoft.CodeAnalysis;

public class SandboxAssemblyBuilder {
	private List<MetadataReference> references = new();

	public void BuildSandboxAssembly() {
		// Get all runtime assemblies
		var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
			.ToList();

		// Add core assemblies for basic types
		foreach (var asm in runtimeAssemblies) {
			var isSystemAssembly = asm.FullName?.StartsWith("System.") == true ||
			                       asm.FullName?.StartsWith("Microsoft.") == true;

			// Skip ONLY system assemblies that contain System.IO types
			// Keep user assemblies (like BlackBox) even if they have System.IO types
			if (isSystemAssembly && asm.GetTypes().Any(t => t.Namespace == "System.IO"))
				continue;

			references.Add(MetadataReference.CreateFromFile(asm.Location));
		}
	}

	public IEnumerable<MetadataReference> GetReferences() => references;
}