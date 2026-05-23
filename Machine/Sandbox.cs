using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Collections.Concurrent;
using System.Utils;

namespace BlackBox.Machine;

public static class Sandbox
{
	private static ScriptOptions scriptOptions;
	private static ScriptState? currentState;
	private static readonly ReaderWriterLockSlim StateLock = new(LockRecursionPolicy.SupportsRecursion);

	private static bool running;
	private static Task? loopTask;
	private static readonly CancellationTokenSource LoopCts = new();
	private static Action? loopAction;
	private static readonly object LoopLock = new();

	public static readonly ConcurrentBag<SubProcess> Processes = new();

	private const string LibrariesPath = "./Files/System/Programs";
	private static SandboxAssemblyBuilder assemblyBuilder = null!;
	private static string[] libraryCodes = [];

	static Sandbox()
	{
		assemblyBuilder = new SandboxAssemblyBuilder();
		assemblyBuilder.BuildSandboxAssembly();

		scriptOptions = ScriptOptions.Default
			.AddReferences(assemblyBuilder.GetReferences())
			.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text")
			.WithAllowUnsafe(false)
			.WithCheckOverflow(true);

		InitializeLibraries();
	}

	private static string[]? LoadLibraryCodes()
	{
		Status.Throw(0, $"[Sandbox] Looking for libraries in: {LibrariesPath}");

		if (!Directory.Exists(LibrariesPath))
		{
			Status.Throw(0, $"[Sandbox] Libraries directory does not exist");
			return null;
		}

		var dirs = Directory.GetDirectories(LibrariesPath);
		Status.Throw(0, $"[Sandbox] Found {dirs.Length} library directories");

		var codes = new List<string>();
		foreach (var dir in dirs)
		{
			var dirName = dir.Split('/').Last();
			var srcFile = $"{dir}/__{dirName}";
			Status.Throw(0, $"[Sandbox] Checking: {srcFile} (exists: {File.Exists(srcFile)})");
			if (File.Exists(srcFile))
			{
				var code = File.ReadAllText(srcFile);
				Status.Throw(0, $"[Sandbox] Loaded '{dirName}' ({code.Length} chars)");
				codes.Add(code);
			}
		}
		return codes.Count > 0 ? codes.ToArray() : null;
	}

	public static void InitializeLibraries()
	{
		var codes = LoadLibraryCodes();
		if (codes == null)
		{
			Status.Throw(0, "[Sandbox] No library code to load");
			return;
		}

		libraryCodes = codes;
		Status.Throw(0, $"[Sandbox] Stored {libraryCodes.Length} libraries for subprocesses");

		Status.Throw(0, $"[Sandbox] Executing library code into main state...");

		StateLock.EnterWriteLock();
		try
		{
			foreach (var code in codes)
			{
				if (currentState == null)
				{
					var script = CSharpScript.Create(code, scriptOptions);
					currentState = script.RunAsync().Result;
				}
				else
					currentState = currentState.ContinueWithAsync(code).Result;
			}
			Status.Throw(0, $"[Sandbox] Libraries loaded successfully. Variables defined: {currentState?.Variables.Length ?? 0}");
			foreach (var v in currentState?.Variables ?? [])
				Status.Throw(0, $"[Sandbox]   - {v.Name}: {v.Type.Name}");
		}
		catch (CompilationErrorException ex)
		{
			Status.Throw(0, $"[Sandbox] Failed to load libraries: {string.Join("\n", ex.Diagnostics)}");
		}
		catch (Exception ex)
		{
			Status.Throw(0, $"[Sandbox] Failed to load libraries: {ex.Message}");
		}
		finally { StateLock.ExitWriteLock(); }
	}

	public static ScriptExecutionResult Execute(string code, object? globals = null, CancellationToken cancellationToken = default)
	{
		try
		{
			ScriptState? stateBeforeExecution;
			ScriptState resultState;

			StateLock.EnterWriteLock();
			try
			{
				stateBeforeExecution = currentState;

				if (currentState == null)
				{
					var script = CSharpScript.Create(code, scriptOptions, globals?.GetType());
					resultState = script.RunAsync(globals, cancellationToken).Result;
				}
				else
					resultState = currentState.ContinueWithAsync(code, cancellationToken: cancellationToken).Result;

				if (currentState == stateBeforeExecution)
					currentState = resultState;
			}
			finally { StateLock.ExitWriteLock(); }

			return new ScriptExecutionResult { Success = true, ReturnValue = currentState!.ReturnValue };
		}
		catch (CompilationErrorException ex)
		{
			return new ScriptExecutionResult { Success = false, Exception = ex, ErrorMessage = string.Join("\n", ex.Diagnostics) };
		}
		catch (Exception ex)
		{
			return new ScriptExecutionResult { Success = false, Exception = ex, ErrorMessage = ex.Message };
		}
	}

	public static ScriptExecutionResult ExecuteFile(string filePath, object? globals = null, CancellationToken cancellationToken = default)
	{
		if (!File.Exists(filePath))
			return new ScriptExecutionResult { Success = false, ErrorMessage = $"File not found: {filePath}" };
		return Execute(File.ReadAllText(filePath), globals, cancellationToken);
	}

	public static void Reset()
	{
		StateLock.EnterWriteLock();
		try { currentState = null; }
		finally { StateLock.ExitWriteLock(); }
	}

	public static void AddReferences(params Assembly[] assemblies) => scriptOptions = scriptOptions.AddReferences(assemblies);
	public static void AddReferences(params Type[] types) => scriptOptions = scriptOptions.AddReferences(types.Select(t => t.Assembly).Distinct());
	public static void AddImports(params string[] namespaces) => scriptOptions = scriptOptions.AddImports(namespaces);

	public static CancellationTokenSource CreateTimeoutToken(TimeSpan timeout)
	{
		var cts = new CancellationTokenSource();
		cts.CancelAfter(timeout);
		return cts;
	}

	public static async Task<T?> Evaluate<T>(string expression, object? globals = null, CancellationToken cancellationToken = default)
	{
		try { return await CSharpScript.EvaluateAsync<T>(expression, scriptOptions, globals, cancellationToken: cancellationToken); }
		catch { return default; }
	}

	public static IEnumerable<ScriptVariable> GetVariables()
	{
		if (currentState == null) return Enumerable.Empty<ScriptVariable>();
		return currentState.Variables.Select(v => new ScriptVariable { Name = v.Name, Type = v.Type, Value = v.Value });
	}

	public static void Run(Action loopAction)
	{
		if (running) throw new InvalidOperationException("Sandbox is already running");

		lock (LoopLock) { Sandbox.loopAction = loopAction; }
		running = true;

		loopTask = Task.Run(() =>
		{
			while (running && !LoopCts.Token.IsCancellationRequested)
			{
				try
				{
					lock (LoopLock) { Sandbox.loopAction?.Invoke(); }
				}
				catch (Exception ex) { Console.Error.WriteLine($"Sandbox loop error: {ex.Message}"); }
			}
		}, LoopCts.Token);
	}

	public static void Run() => Run(() => { });

	public static void Stop()
	{
		running = false;
		LoopCts.Cancel();
	}

	public static async Task WaitForStop() { if (loopTask != null) await loopTask; }
	public static bool IsRunning => running;

	public static SubProcess Spawn(string name, string code, object? globals = null)
	{
		var process = new SubProcess(name, code, scriptOptions, globals, SubProcess.DefaultErrorHandler, libraryCodes);
		Processes.Add(process);
		process.Start();
		return process;
	}

	internal static SubProcess SpawnInit(string code, object? globals = null)
	{
		var process = new SubProcess("Init", code, scriptOptions, globals, SubProcess.InitErrorHandler, libraryCodes, GUID.V8(Host.Random, 8, 2, 1, 0));
		Processes.Add(process);
		process.Start();
		return process;
	}

	public static async Task<SubProcess?> SpawnFile(string name, string filePath, object? globals = null)
	{
		if (!File.Exists(filePath)) return null;
		return Spawn(name, await File.ReadAllTextAsync(filePath), globals);
	}
}

public class ScriptExecutionResult
{
	public bool Success { get; set; }
	public object? ReturnValue { get; set; }
	public Exception? Exception { get; set; }
	public string? ErrorMessage { get; set; }
}

public class ScriptVariable
{
	public string Name { get; set; } = "";
	public Type Type { get; set; } = typeof(object);
	public object? Value { get; set; }
}

public enum ProcessState { Starting, Running, Exited }

public delegate void ProcessErrorHandler(string name, GUID guid, Exception ex, string errorMessage);

public class SubProcess
{
	private static readonly AsyncLocal<SubProcess?> current = new();
	public static SubProcess? Current => current.Value;

	private readonly string code;
	private readonly string[] libraryCodes;
	private readonly ScriptOptions options;
	private readonly object? globals;
	private readonly CancellationTokenSource cts = new();
	private readonly TaskCompletionSource<ScriptExecutionResult> completionSource = new();
	private readonly ProcessErrorHandler errorHandler;

	private ProcessState state = ProcessState.Starting;
	private ScriptExecutionResult? result;
	private DateTime startTime;
	private DateTime? endTime;

	public ConcurrentQueue<Message> Messages = new();

	public string Name;
	public GUID GUID;

	internal static readonly ProcessErrorHandler DefaultErrorHandler = (name, guid, ex, msg) =>
	{
		// Stub: default subprocess error handler
		Status.Throw(3, guid, name, $"Fatal: {ex}");
	};

	internal static readonly ProcessErrorHandler InitErrorHandler = (name, guid, ex, msg) =>
	{
		// Stub: init process error handler
		Status.Throw(4, $"[INIT] Fatal: {ex}");
		System.Terminal.SetRow(3, $"[INIT] Fatal: {msg}");
	};

	internal SubProcess(string name, string code, ScriptOptions options, object? globals, ProcessErrorHandler errorHandler, string[]? libraryCodes = null, GUID? guid = null)
	{
		this.code = code;
		this.libraryCodes = libraryCodes ?? [];
		this.options = options;
		this.globals = globals;
		this.errorHandler = errorHandler;

		Name = name;
		GUID = guid ?? GUID.V8(Host.Random, 0, 2, 0, 0);

		Process.Processes.Add(this);
	}

	public void Start()
	{
		startTime = DateTime.UtcNow;
		state = ProcessState.Running;

		_ = Task.Run((Func<Task>)(async () =>
		{
			current.Value = this;
			try
			{
				ScriptState? scriptState = null;

				// Execute library codes first
				foreach (var libCode in libraryCodes)
				{
					if (scriptState == null)
					{
						var libScript = CSharpScript.Create(libCode, options);
						scriptState = await libScript.RunAsync(cancellationToken: cts.Token);
					}
					else
						scriptState = await scriptState.ContinueWithAsync(libCode, cancellationToken: cts.Token);
				}

				// Continue with main code
				if (scriptState != null)
					scriptState = await scriptState.ContinueWithAsync(code, cancellationToken: cts.Token);
				else
				{
					var script = CSharpScript.Create(code, options, globals?.GetType());
					scriptState = await script.RunAsync(globals, cts.Token);
				}

				result = new ScriptExecutionResult { Success = true, ReturnValue = scriptState.ReturnValue };
			}
			catch (CompilationErrorException ex)
			{
				var errorMsg = string.Join("\n", ex.Diagnostics);
				errorHandler(Name, GUID, ex, errorMsg);
				result = new ScriptExecutionResult { Success = false, Exception = ex, ErrorMessage = errorMsg };
			}
			catch (Exception ex)
			{
				errorHandler(Name, GUID, ex, ex.Message);
				result = new ScriptExecutionResult { Success = false, Exception = ex, ErrorMessage = ex.Message };
			}
			finally
			{
				state = ProcessState.Exited;
				endTime = DateTime.UtcNow;
				completionSource.TrySetResult(result!);
				Process.Processes.Remove(this);
			}
		}), cts.Token);
	}

	public void Stop() => cts.Cancel();
	public ProcessState State => state;
	public ScriptExecutionResult? Result => result;
	public DateTime StartTime => startTime;
	public DateTime? EndTime => endTime;
	public Task<ScriptExecutionResult> WaitForCompletion() => completionSource.Task;
}
