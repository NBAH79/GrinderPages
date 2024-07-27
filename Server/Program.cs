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
using System.Diagnostics;
using Server;


public static class Global
    {
        //public static CancellationTokenSource Token = new CancellationTokenSource();
        public static string WWW {get;set;}= "http://localhost:8080/"; //82 порт не будет работать потому что на нем WebSocket!
        public static string XXX {get;set;}= "ws://127.0.0.1:82"; //wss 443
        public static string YYY {get;set;}= "ws://127.0.0.1:82"; //? будет ли wss 443
        public static string IP {get;set;}= "127.0.0.1";
        public static int PORT {get;set;}= 82;

        public static string website = "Server\\WebSite";

        //если 443, то и wss дописать и сертификат надо!
    };

public class Program
{
    //public const string path = "Server\\WebSite"; //win
    //public const string path = "";   //linux



    // public class Tools
    // {

    //     public string HtmlTagOpen(Model model) => model == null ? string.Empty : $"<{model.Tag} id=\"{model.Id}\" class=\"{model.Class}\" style=\"{model.Style}\" />";
    //     public string HtmlTagClose(Model model) => model == null ? string.Empty : $"</{model.Tag}>";

    // }
    
    public class Manager : IManager
    {
        //public List<Model> models = new List<Model>();
        //public List<IService> services = new List<IService>();
        //public Global global { get; set; } = new Global_();
        public string globalWWW {get;set;}
        public Listener listener {get;set;}=new Listener();
        public List<Page> pages = new List<Page>();
        public List<Assembly> assemblies = new List<Assembly>(); //подключение dll

        public Assembly? GetAssembly(string shortname)
        {
            return assemblies.FirstOrDefault<Assembly>(x => string.Compare(x.GetName().Name, shortname) == 0);
        }
        // public Type? GetClass(string shortname, string fullname) { 
        //     var a=assemblies.FirstOrDefault<Assembly>(x=>string.Compare(x.GetName().Name,shortname)==0);
        //     return a?.GetType(fullname);
        //      }
        //  public MethodInfo? GetMethod(Type type, string name, bool instance, Type[] parameters) {
        //       return type.GetMethod(name,BindingFlags.Public | (instance?BindingFlags.Instance:BindingFlags.Static),null, CallingConventions.Any,parameters,null);
        //  }

        // public Model? GetModel(int id){
        //     return models.FirstOrDefault(x=>x._id==id);
        // }

        public Instance? GetInstance(string shortname, string classfullname)
        {
            var a = GetAssembly(shortname);//assemblies.FirstOrDefault<Assembly>(x=>string.Compare(x.GetName().Name,shortname)==0);
            var t = a?.GetType(classfullname);
            ConstructorInfo? constructor = t?.GetConstructor(Type.EmptyTypes);
            return new Instance() { type = t, obj = constructor?.Invoke(new object[] { }) };
        }

        public Type? GetStatic(string shortname, string classfullname)
        {
            var a = GetAssembly(shortname);//assemblies.FirstOrDefault<Assembly>(x=>string.Compare(x.GetName().Name,shortname)==0);
            return a?.GetType(classfullname);
        }

        //1 миллисекунда вместе с Instance
        public T Invoke<T>(Instance? instance, string name, Type[] parametertypes, object[] parameters)
        {
            var m = instance?.GetMethod(name, false, parametertypes);
            object? ret = m?.Invoke(instance?.obj, parameters);
            return (T)(ret ?? default(T));
        }

        //примерно в 4 раза быстрее обычного Invoke, 0.25 миллисекунды
        public T InvokeStatic<T>(Type? type, string name, Type[] parametertypes, object[] parameters)
        {
            var m = type?.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, parametertypes, null);
            object? ret = m?.Invoke(null, parameters);
            return (T)(ret ?? default(T));
        }

        //public MethodInfo? GetMethod(string fullobjectname,string name){return new MethodInfo();}

        //Item i = System.Activator.CreateInstance<Item>(obj);
        public async Task SendText(Stream stream, string text) { await Listener.SendText(stream,text); 
        /*Console.Write("OUTPUT:" + text); await Task.CompletedTask;*/ }
    }


    public class Call<T>
    {
        public T? t;
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
    static async Task Main(string[] args)
    {
        //Global_ global = new Global_();

        foreach (var a in args)
        {
            string parameter = a.TrimEnd('=');
            string value = a.TrimStart('=');
            switch (parameter)
            {
                case "-www": Global.WWW = value; break;
                case "-wss":
                    Global.XXX = Global.YYY = value;
                    Global.PORT = int.Parse(value.TrimEnd(':'));
                    break;
                case "-website":
                    Global.website = value;
                    break;
                case "-?":
                    Console.WriteLine("Аvailable parameters:\n-www: http server address with port\n-wss: websocket address\n-website: website base directory");
                    Console.WriteLine("Еxample: server.exe -www=http://localhost:8080/ -wss=ws://127.0.0.1:82 -website=Server\\WebSite");
                    break;
                default: Console.WriteLine($"bad parameter:{parameter}\ntype -? for help"); break;
            };
        }

        //Tools tools = new Tools();
        Manager manager = new Manager();
        manager.globalWWW=Global.WWW;
        //manager.global = global;

        Console.WriteLine("\nGRINDER SERVER\nPress ctrl+c to stop");

        //var d=File.CreateText("_path");
        //d.WriteLine("base directory");
        //d.Close();

        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            Console.WriteLine("\nStopped by user");
            GlobalToken.Cancel(true);
        };
        //Console.WriteLine(">>> READY <<<");
        //Server.Listener listener = new Server.Listener();
        //manager.listener=listener;

        manager.listener.RegisterService(new PongService(manager));
        manager.listener.RegisterService(new EchoService(manager));
        manager.listener.RegisterService(new EventService(manager));
        manager.listener.RegisterService(new UrlService(manager));

        var taskListener = Task.Factory.StartNew(async () => await Rebuild(manager));
        //Console.WriteLine(">>> FINISHED <<<");

        await manager.listener.RunAsync(
                manager,
                Global.WWW,
                IPAddress.Parse(Global.IP),
                Global.PORT,
                Global.website + "\\meta.html",
                GlobalToken.Token
                );

        GlobalToken.Cancel();
        Task.WaitAny(taskListener);
        await Task.CompletedTask;
    }

    public static async Task Rebuild(Manager manager)
    {
        FileSystemWatcher watcher = new FileSystemWatcher();

        watcher.Path = Global.website;

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

        //Call<Model> callmodel = new Call<Model>() { manager = manager };
        Call<Page> callpage = new Call<Page>() { manager = manager };


        while (!GlobalToken.IsCancellationRequested)
        {
            await Task.Delay(3000);
            try
            {
                if (recompile)
                {
                    //globals.input = "some lowercase text";
                    //var options = ScriptOptions.Default.WithImports("System.IO", "System.Exception");

                    //public static List<Script> models = new List<Script>();

                    //ublic static List<Script> services = new List<Script>();



                    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

                    foreach (var f in Directory.GetFiles($"{Global.website}/dlls/", "*.dll"))//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..."));
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
                    // manager.models.Clear();

                    // foreach (var f in Directory.GetFiles($"{path}/components/"))
                    // {
                    //     Script model = CSharpScript.Create(File.ReadAllText(f), options, typeof(Call<Model>), null);
                    //     model.RunAsync(globals: callmodel, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                    //     if (callmodel.t == null) break;
                    //     else manager.models.Add(callmodel.t);
                    //     Console.WriteLine("model id:" + callmodel.t._id);
                    // }

                    manager.pages.Clear();
                    foreach (var f in Directory.GetFiles($"{Global.website}/pages/"))
                    {
                        Script service = CSharpScript.Create(File.ReadAllText(f), options, typeof(Call<Page>), null);
                        service.RunAsync(globals: callpage, catchException: catchException, new CancellationToken()).GetAwaiter().GetResult();
                        if (callpage.t == null) break;
                        else manager.pages.Add(callpage.t);
                        Console.WriteLine("page id:" + callpage.t._id);
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
                    // foreach (var m in manager.models)
                    // {
                    //     Console.WriteLine($"{tools.HtmlTagOpen(m)} - {tools.HtmlTagClose(m)}");
                    // }


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
                Console.WriteLine("Exception:" + ex);
                await Task.Delay(10000);
            }
            await Task.CompletedTask;
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

    public class PongService : Server.IService
    {
        public override void Register(Server.Listener listener)
        {
            listener.parser.OnPong += OnPong;
            //sendPing = grinder.SendPing;
        }
        public PongService(IManager manager) {this.manager=manager; }
        //public SendPong sendPing = (async (a, b) => { return await Task.FromResult(false); });

        public async Task OnPong(Server.Session session, byte[] data, uint id)// byte[] frame, int opcode, ulong pos, ulong len)
        {
            var pong = new Server.Listener.Pong(data);
            if (pong.IsCorrect) Console.WriteLine($"Pong: {id} ({pong.ping}) ");
            else Console.WriteLine($"Pong: error data! ");
            await Task.CompletedTask;
        }
    }

    // этот сервис отправляет страницу если запрос был именно URL
    // теоретически у сервера могут спрашивать и другие системы, например приложение
    // тогда надо высылать данные соответственно логике этого приложения
    public class UrlService : Server.IService
    {
        public override void Register(Server.Listener listener)
        {
            listener.parser.OnLson += OnLson;
            listener.parser.OnUpdate += OnUpdate;
        }

        public UrlService(IManager manager) {this.manager=manager; }

        public async Task OnLson(Server.Session session, string text)// byte[] frame, int opcode, ulong pos, ulong len)
        {
            //string text = Encoding.UTF8.GetString(msg.data);
            string[] operands = Lson.Parse(text);
            if (operands.Length == 0) return;
            var _command = operands[0];


            //// тут еще page release с таймером

            if (operands[0] == "URL") //operands.length==1
            {
                Console.WriteLine($"Requested page: {text}");
                //var _url = (operands.Length > 1) ? operands[1] : Global.WWW + "Err404.html";
                var _url = (operands.Length > 1) ? operands[1] :"Err404.html";
                //if (operands.Length<2) return await Task.FromResult(false);//{
                //    //переход на главную
                //}
                var _operands = _url.Split('?');
                var _location = _operands[0];
                //if (string.Compare(_location, Global.WWW) == 0) _location = Global.WWW + "Index.html";
                if (string.Compare(_location, Global.WWW) == 0) _location = "Index.html";
                var _parameters = (operands.Length > 1) ? operands[1] : "";

                //if (_location==CurrentPage.Url) await Task.FromResult(true); //а может рефреш?

                var p=_location.TrimEnd('/');
                Manager m=manager as Manager;
                var grinderpage = m.pages.Find(x=>string.Compare(x._id, p)==0);
                if (grinderpage==null) {
                    Console.WriteLine($"Location not found: {_location} {p}");
                    return;
                }
                session.page=grinderpage;
                //session.page = Program.InstantiatePage(_location);// .Find(x => string.Compare(x.Url, _location) == 0);

                //if (p == null) session.page = templates[0].Instantiate(); //404
                //else session.page = p.Instantiate();

                //await CurrentPage.OnInitialize();
                await session.page.Create(session.client.GetStream(), _url, new Dictionary<string, string>());
                //await CurrentPage.OnParametersSet("");
                //Console.WriteLine($"Error404: ({operands.Length})");
                Console.WriteLine($"Location: ({operands.Length}) {_parameters}");

                //существующая страница может принять другие параметры
                //if (param.Length > 1)
                //{ //а хрен его знает сколько этих '?' там
                //    var parameters = Lson.Url(param[1]);
                //    if (parameters.Count > 0) await CurrentPage.OnParametersSet(parameters);
                //    await CurrentPage.OnRender(stream, param[0], parameters);
                //    Console.WriteLine($"Parameters: ({operands.Length}) {param[1]}");
                //}
                //else await CurrentPage.OnRender(stream, param[0], new Dictionary<string, string>());
                return;

            }
            if (operands[0] == "UPD")
            {
                if (session.page == null) return;
                //await session.page.Update();
                await session.page.Render(session.client.GetStream(), "", true, new Dictionary<string, string>());
                return;
            }
            //if (operands[0] == "REL")
            //{
            //    session.page.Release(session.client.GetStream());
            //}
            if (operands.Length > 1 && session.page != null) await session.page.Event(session.client.GetStream(), operands);
            return;
        }
        public async Task OnUpdate(Server.Session session)// byte[] frame, int opcode, ulong pos, ulong len)
        {
            if (session.page == null) return;
            //await session.page.Update();
            await session.page.Render(session.client.GetStream(), "", false, new Dictionary<string, string>());
        }

        public static string? GetOperand(string[] operands, int n, string? def = null)
        {
            if (operands.Count() > n) return operands[n];
            return def;
        }

    }


    public class EchoService : Server.IService
    {
        //по логике передача файла должна начинаться с текстового соообщения, в котором сказано название файла и инициирован прием
        //вся сессия переключается на прием данных, и если это текст - значит что то пошло не так
        //если данные прервались - разорвать соединение

        public override void Register(Server.Listener listener)
        {
            listener.parser.OnData += OnData;
        }

        public EchoService(IManager manager) {this.manager=manager; }
        public async Task OnData(Server.Session session, Server.Transfer transfer)// byte[] frame, int opcode, ulong pos, ulong len)
        {
            Console.WriteLine($"({transfer.position} of {transfer.length}) ");
            try
            {
                await Server.Listener.SendData(session.client.GetStream(), transfer, 1);
            }
            catch { }
        }
    }

    public class EventService : Server.IService
    {
        //Button button=new ();
        //Input input=new ();

        public override void Register(Server.Listener listener)
        {
            listener.parser.OnLson += OnLson;
        }
        public EventService(IManager manager) {this.manager=manager; }   
        public async Task OnLson(Server.Session session, string text)// byte[] frame, int opcode, ulong pos, ulong len)
        {
            //string text = Encoding.UTF8.GetString(msg.data);
            string[] operands = Lson.Parse(text);

            await Task.CompletedTask;
            //Console.WriteLine($"Text: ({operands.Count()})");
            //switch (operands[0])
            //{
            //    case string when string.IsNullOrEmpty(text):
            //        break;
            //    case "button":
            //        {
            //            if (operands.Count() == 1) await button.Destroy(stream, 3000);
            //            else
            //            {
            //                string? temp = GetOperand(operands, 1);
            //                if (!string.IsNullOrEmpty(temp)) button.height = temp;
            //                temp = GetOperand(operands, 2);
            //                if (!string.IsNullOrEmpty(temp)) button.title = temp;
            //                button.opacity = "1";
            //                button.Class = "two";
            //                button.OnUpdate();
            //                await button.Render(stream,true,false);// $"<button style='width:200px;height:{operands[1]}'>BUTTON</button>");
            //            }
            //        }
            //        break;
            //    case "input":
            //        {
            //            string? temp = GetOperand(operands, 1);
            //            if (!string.IsNullOrEmpty(temp)) input.type = temp;
            //            temp = GetOperand(operands, 2);
            //            if (!string.IsNullOrEmpty(temp)) input.placeholder = temp;
            //            await input.Render(stream,true);
            //            // "<input style='width:200px;height:48px' type='text' placeholder='some text'><label>INPUT</label></input>");
            //        }
            //        break;
            //    default:
            //        //byte[] data = Encoding.UTF8.GetBytes(__t);
            //        //foreach (var d in data) Global.console.WriteLine($"{d:X}");
            //        //await Node.Send2(__s, "<p>UNKNOWN COMMAND</p>");
            //        await SendText(stream, operands[0]);
            //        break;
            //}
        }

        public static string? GetOperand(string[] operands, int n, string? def = null)
        {
            if (operands.Count() > n) return operands[n];
            return def;
        }
    }

}