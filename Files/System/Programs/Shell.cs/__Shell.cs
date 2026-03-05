Terminal.WriteLine("dfi");

//repl logic goes here

foreach (var (key, value) in Process.Processes)
{
    Terminal.Write((string)key + value.pid)
}