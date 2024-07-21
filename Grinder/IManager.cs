using System.Reflection;

namespace Grinder;


public class Instance
{
    public Type? type;
    public object? obj;
    public MethodInfo? GetMethod(string name, bool instance, Type[] parameters)
    {
        return type?.GetMethod(
            name,
                BindingFlags.Public | 
                (instance ? BindingFlags.Instance : BindingFlags.Static),
            null,
            CallingConventions.Any,
            parameters,
            null);
    }
}

// public abstract class Global
//  {
//      //public static CancellationTokenSource Token = new CancellationTokenSource();
//      public string WWW {get;set;}
//      public string XXX {get;set;}
//      public string YYY {get;set;}
//      public string IP {get;set;}
//      public int PORT {get;set;} //если 443, то и wss дописать и сертификат надо!
//  };
public interface IManager
{
    //Assembly? GetAssembly(string filename);
    // Type? GetClass(Assembly assembly,string fullname); //лучше для оптимизации
    //Type? GetClass(string shortname,string fullname); //лучше для оптимизации
    //MethodInfo? GetMethod(Type type,string name, Type[] parameters);
    //MethodInfo GetMethod(string assemblyname, string fulltypename,string methodname); //компактный
    //public Model? GetModel(int id);
    //public Global global{get;set;}
    public Instance? GetInstance(string shortname, string fullname);
    public Type? GetStatic(string shortname, string fullname);
    public T Invoke<T>(Instance? instance, string name, Type[] parametertypes, object[] parameters);
    public T InvokeStatic<T>(Type? type, string name, Type[] parametertypes, object[] parameters);

    public Task SendText(Stream stream, string text);
}


