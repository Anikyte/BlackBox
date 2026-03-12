Terminal.WriteLine("Shell Loaded");
Console.WriteLine("Shell Loaded");

//repl logic goes here

foreach (var (key, value) in Process.Processes)
{
    Terminal.Write(key.ToString() + value.pid.ToString());
}