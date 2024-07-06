using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;
using System.Reflection;
using System.Security.Principal;
using System.IO.Enumeration;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32.SafeHandles;
class Program
{
    public const string path = @"../../../pages/";


    public static CancellationTokenSource GlobalToken = new CancellationTokenSource();
    public static volatile bool recompile = true;
    static void Main(string[] args)
    {
        Console.WriteLine("\nGRINDER SERVER\nPress ctrl+c to stop");

        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            Console.WriteLine("\nStopped by user");
            GlobalToken.Cancel(true);
        };

        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = path;

        //// watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
        ////    | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        ////watcher.Filter = "*.py";

        // Add event handlers.
        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnChanged;
        ////watcher.Error += OnChanged;

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        while (!GlobalToken.IsCancellationRequested)
        {
            Task.Delay(1000);
            try
            {
                if (recompile)
                {
                    var f=File.OpenRead($"{path}script.csx");
                    //var script=File.ReadAllText();
                    var script=CSharpScript.Create(f);
                    f.Close();
                    script.Compile();
                    script.RunAsync().GetAwaiter().GetResult();
                    recompile = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed) return;
        if (!recompile) recompile=true;
    }

    
}