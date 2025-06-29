#!/usr/bin/env dotnet-script

using System.Diagnostics;
using System.Threading;

public class Main
{

  public static void Run()
  {
    Console.Write("\n🟨 Waiting for postgres");
    Check();
  }

  public static void Check()
  {
    var output = ExecuteDockerCheck("docker", "exec the-chatbot-pg pg_isready --host localhost");
    if (output.Contains("accepting connections"))
    {
      Console.WriteLine("\n✅ Postgres is accepting connections\n");
      return;
    }
    Console.Write(".");
    Thread.Sleep(350);
    Check();
  }
  public static string ExecuteDockerCheck(string command, string arguments)
  {
    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "docker",
        Arguments = "exec the-chatbot-pg pg_isready --host localhost",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
      }
    };

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return string.IsNullOrEmpty(error) ? output : error;
  }

}

Main.Run();
