#!/usr/bin/env dotnet-script

using System.Diagnostics;
using System.Threading;


string ExecuteCommand(string command, string arguments)
{
  var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = command,
      Arguments = arguments,
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true,
    }
  };

  process.Start();
  string output = process.StandardOutput.ReadToEnd();
  process.WaitForExit();
  return output;
}

void Check()
{
  Console.Write("\n🟨 Waiting for postgres");
  while (true)
  {
    var output = ExecuteCommand("docker", "exec the-chatbot-pg pg_isready --host localhost");
    if (output.Contains("accepting connections"))
    {
      Console.WriteLine("\n✅ Postgres is accepting connections\n");
      return;
    }

    Console.Write(".");
    Thread.Sleep(350);
  }
}

Check();

