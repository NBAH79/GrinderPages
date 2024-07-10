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
using System.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography.X509Certificates;
using Grinder;
using System.Security.AccessControl;

public class Program
{
    public const string path = "../../../"; //win
                                                  //public const string path = "";   //linux

    public static string ModelToHtml(IModel model){

        return model==null? string.Empty: $"<{model.Tag} id=\"{model.Id}\" class=\"{model.Class}\" style=\"{model.Style}\" />";

    }                                      
    public class Params
    {
        public int id;
        public IController? page;
    };

    public class Model
    {
        public int uid;
        public IModel? model;

        public object x=new object();

    }

    public static Model models = new Model(){x="width:100px;height:100px;"};


    public static CancellationTokenSource GlobalToken = new CancellationTokenSource();
    public static volatile bool recompile = true;
    static void Main(string[] args)
    {
        Console.WriteLine("\nGRINDER SERVER\nPress ctrl+c to stop");
        //       var d=File.CreateText("_path");
        //       d.WriteLine("base directory");
        //       d.Close();

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

        //Params globals = new Params();
        //globals.input = "Hello global!";

        Func<Exception, bool> catchException = ((e) => { Console.WriteLine(e.Message); return true; });
        var options = ScriptOptions.Default
            .AddImports("System", "System.IO", "System.Collections.Generic",
                "System.Console", "System.Diagnostics", "System.Dynamic",
                "System.Linq", "System.Text",
                "System.Threading.Tasks","Grinder")
            .AddReferences("System", "System.Core", "Microsoft.CSharp", "Grinder");

        while (!GlobalToken.IsCancellationRequested)
        {
            Task.Delay(1000);
            try
            {
                if (recompile)
                {
                    //globals.input = "some lowercase text";
                    //var options = ScriptOptions.Default.WithImports("System.IO", "System.Exception");
                    Script<object> script = CSharpScript.Create(File.ReadAllText($"{path}/models/container.csx"), options, typeof(Model), null);
                    //CSharpScript.EvaluateAsync(File.ReadAllText($"{path}script.csx"), options, globals, typeof(Params), new CancellationToken());
                    //script.Compile();
                    ScriptState<object> state = script.RunAsync(globals: models, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                    foreach(var variable in state.Variables)
{
	Console.WriteLine($"{variable.Name} - {variable.Value}");
}
                    //var type=state.GetType("one");
                    //var o=state.GetVariable("o");

                    Console.WriteLine(ModelToHtml(models.model));
//Console.WriteLine(globals.page?.OnInitialize("",new Dictionary<string,string>()));


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
        if (!recompile) recompile = true;
    }


}