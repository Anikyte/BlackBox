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

	static Sandbox()
	{
		var assemblyBuilder = new SandboxAssemblyBuilder();
		assemblyBuilder.BuildSandboxAssembly();

		scriptOptions = ScriptOptions.Default
			.AddReferences(assemblyBuilder.GetReferences())
			.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text")
			.WithAllowUnsafe(false)
			.WithCheckOverflow(true);
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
					CleanupDeadProcesses();
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

	public static SubProcess Spawn(string code, object? globals = null)
	{
		var process = new SubProcess(code, scriptOptions, globals);
		Processes.Add(process);
		process.Start();
		return process;
	}

	public static async Task<SubProcess?> SpawnFile(string filePath, object? globals = null)
	{
		if (!File.Exists(filePath)) return null;
		return Spawn(await File.ReadAllTextAsync(filePath), globals);
	}

	private static void CleanupDeadProcesses()
	{
		// ConcurrentBag doesn't support removal, dead processes stay until collection is rebuilt
		// This is acceptable since processes are short-lived and bag is periodically cleared
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

public class SubProcess
{
	private static readonly AsyncLocal<SubProcess?> current = new();
	public static SubProcess? Current => current.Value;

	private readonly string code;
	private readonly ScriptOptions options;
	private readonly object? globals;
	private readonly CancellationTokenSource cts = new();
	private readonly TaskCompletionSource<ScriptExecutionResult> completionSource = new();

	private ProcessState state = ProcessState.Starting;
	private ScriptExecutionResult? result;
	private DateTime startTime;
	private DateTime? endTime;

	public ConcurrentQueue<Message> Messages = new();
	public GUID GUID;

	internal SubProcess(string code, ScriptOptions options, object? globals)
	{
		this.code = code;
		this.options = options;
		this.globals = globals;

		GUID = GUID.V8(new Random(), 0, 2, 0, 0);
		
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
				var script = CSharpScript.Create(code, options, globals?.GetType());
				var scriptState = await script.RunAsync(globals, cts.Token);
				result = new ScriptExecutionResult { Success = true, ReturnValue = scriptState.ReturnValue };
			}
			catch (CompilationErrorException ex)
			{
				var errorMsg = string.Join("\n", ex.Diagnostics);
				Console.Error.WriteLine($"Compilation Error: {errorMsg}");
				System.Terminal.Write($"Compilation Error: {errorMsg}\n");
				result = new ScriptExecutionResult { Success = false, Exception = ex, ErrorMessage = errorMsg };
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Runtime Error: {ex.Message}");
				System.Terminal.Write($"Runtime Error: {ex.Message}\n");
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
