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
//using UserLibrary;
using System.Security.AccessControl;
using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

public class Program
{
    public const string path = "../../../"; //win
                                            //public const string path = "";   //linux



    public class Tools
    {

        public string HtmlTagOpen(IModel model) => model == null ? string.Empty : $"<{model.Tag} id=\"{model.Id}\" class=\"{model.Class}\" style=\"{model.Style}\" />";
        public string HtmlTagClose(IModel model) => model == null ? string.Empty : $"</{model.Tag}>";

    }

    public class Manager : IManager
    {
        public List<IModel> models = new List<IModel>();
        //здесь должна быть какая то обертка модели с названием файла или идентификатором из файла
        //у каждой модели может быть функция реакции на события
        //void OnEvent(object e);
        public List<IService> services = new List<IService>();

        public List<Assembly> assemblies = new List<Assembly>(); //подключение dll

        // public MethodInfo? GetMethod(string assembly, string classname, string method)
        // {  //type это classname или "Program"
        //     Assembly? a = assemblies.Find(x => string.Equals(x.GetName().Name, assembly));
        //     return a?.GetType(classname)?.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
        // }
        public Assembly GetAssembly(string filename) { return Assembly.LoadFrom(Path.GetFullPath(filename)); }
        public Type? GetClass(Assembly assembly, string fullname) { 
            return assembly.GetType(fullname);
             }
        public MethodInfo? GetMethod(Type type, string name, Type[] parameters) { return type.GetMethod(name,BindingFlags.Public | BindingFlags.Instance,null, CallingConventions.Any,parameters,null); }
        //public MethodInfo? GetMethod(string fullobjectname,string name){return new MethodInfo();}
   
   //Item i = System.Activator.CreateInstance<Item>(obj);
    }


    public class Call<T>
    {
        public T? t;

        public object x = new object();
    }
    public class Call2<T>
    {
        public T? ret;

        public Manager? manager;

    }

    //Manager


    // public class CallToService
    // {
    //     public string name; 
    //     public IService service;

    //     public object x=new object();
    // }
    // public class Tag{
    //     public IModel model;
    //     public List<IModel> children;

    // }

    //// pages
    //// scripts

    public static CancellationTokenSource GlobalToken = new CancellationTokenSource();
    public static volatile bool recompile = true;
    static void Main(string[] args)
    {

        Tools tools = new Tools();
        Manager manager = new Manager();

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
        ////watcher.Filter = "*.csx";

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
                "System.Threading.Tasks", "Grinder")
            .AddReferences("System", "System.Core", "Microsoft.CSharp", "Grinder");

        // // Assembly asm = Assembly.LoadFrom("MyApp.dll");

        // // Type? t = asm.GetType("Program");
        // // if (t is not null)
        // // {
        // //     // получаем метод Square
        // //     MethodInfo? square = t.GetMethod("Square", BindingFlags.NonPublic | BindingFlags.Static);

        // //     // вызываем метод, передаем ему значения для параметров и получаем результат
        // //     object? result = square?.Invoke(null, new object[] { 7 });
        // //     Console.WriteLine(result); // 49
        // // } 

        while (!GlobalToken.IsCancellationRequested)
        {
            Task.Delay(3000);
            try
            {
                if (recompile)
                {
                    //globals.input = "some lowercase text";
                    //var options = ScriptOptions.Default.WithImports("System.IO", "System.Exception");

                    //public static List<Script> models = new List<Script>();

                    //ublic static List<Script> services = new List<Script>();
                    Call2<IModel> callmodel = new Call2<IModel>() { manager = manager };
                    Call<IService> callscript = new Call<IService>();

                    manager.models.Clear();

                    foreach (var f in Directory.GetFiles($"{path}/models/"))
                    {
                        Script model = CSharpScript.Create(File.ReadAllText(f), options, typeof(Call2<IModel>), null);
                        model.RunAsync(globals: callmodel, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                        if (callmodel.ret == null) break;
                        else manager.models.Add(callmodel.ret);
                        Console.WriteLine("model id:"+callmodel.ret._id);
                    }

                    foreach (var f in Directory.GetFiles($"{path}/services/"))
                    {
                        Script service = CSharpScript.Create(File.ReadAllText(f), options, typeof(Call<IService>), null);
                        service.RunAsync(globals: callscript, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                        if (callscript.t == null) break;
                        else manager.services.Add(callscript.t);
                    }

                    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

                    foreach (var f in Directory.GetFiles($"{path}/dlls/", "*.dll"))//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..."));
                    {

                        var x = Assembly.LoadFrom(Path.GetFullPath(f));
                        Type[] types = x.GetTypes();
                        foreach (Type t in types)
                        {
                            //Console.WriteLine(t.Name + "-");
                            MethodInfo[] methods = t.GetMethods();
                            foreach (MethodInfo i in methods)
                            {
                                //Console.WriteLine(i.Name + "--");

                            }

                            ConstructorInfo? constructor = t?.GetConstructor(Type.EmptyTypes);
                            object? classObject = constructor?.Invoke(new object[] { });


                        }
                        var type = x.GetType("UserLibrary.Class1");
                        //много
                        if (type != null)
                        {
                            object? obj = Instantiate(type, new object[] { });
                            var method = type.GetMethod("Method1", BindingFlags.Public | BindingFlags.Instance,
        null,
        CallingConventions.Any,
        new Type[] { typeof(string) },
        null);// BindingFlags.NonPublic | BindingFlags.Static);
                            method?.Invoke(obj, new object[] { "thee" });
                        }
                        // ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                        //     object classObject = constructor.Invoke(new object[] { });
                        // Console.WriteLine(x.GetType("UserLibrary.Class1")?.Name + "+");
                        // Console.WriteLine(x.GetType("UserLibrary.Class1")?.GetMethod("Method1", BindingFlags.NonPublic | BindingFlags.Static)?.Name + "++");
                        // x.GetType("UserLibrary.Class1")?.GetMethod("Method1", BindingFlags.NonPublic | BindingFlags.Static)?
                        // .Invoke(null, new object[] { new string[] { "thee" } });
                        manager.assemblies.Add(x);
                    }
                    // try{
                    //     Call<IModel> model=new Call<IModel>(++counter);
                    //     script.RunAsync(globals: model, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                    //     models.Add(model.t);
                    // }
                    // catch(Exception e)
                    // {
                    //     Console.WriteLine(e.Message);
                    // };


                    //int counter=0;
                    foreach (var m in manager.models)
                    {
                        Console.WriteLine($"{tools.HtmlTagOpen(m)} - {tools.HtmlTagClose(m)}");
                    }


                    //Script<object> script = CSharpScript.Create(File.ReadAllText($"{path}/models/container.csx"), options, typeof(Model), null);
                    //CSharpScript.EvaluateAsync(File.ReadAllText($"{path}script.csx"), options, globals, typeof(Params), new CancellationToken());
                    //script.Compile();
                    ////ScriptState<object> state = script.RunAsync(globals: models, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                    ////foreach(var variable in state.Variables)
                    ////{
                    ////Console.WriteLine($"{variable.Name} - {variable.Value}");
                    ////}
                    //var type=state.GetType("one");
                    //var o=state.GetVariable("o");

                    ////Console.WriteLine(ModelToHtml(models.model));
                    ////Console.WriteLine(globals.page?.OnInitialize("",new Dictionary<string,string>()));


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

    public static object? Instantiate(Type type, object?[] parameters)
    {
        ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
        return constructor?.Invoke(parameters);
    }
}