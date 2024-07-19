using System.Reflection;

namespace Grinder;

public interface IManager
{
    //Assembly? GetAssembly(string filename);
    // Type? GetClass(Assembly assembly,string fullname); //лучше для оптимизации
    //Type? GetClass(string shortname,string fullname); //лучше для оптимизации
    //MethodInfo? GetMethod(Type type,string name, Type[] parameters);
    //MethodInfo GetMethod(string assemblyname, string fulltypename,string methodname); //компактный

    public (Type? t,object? obj) Instance(string shortname, string fullname);
    public Type? Static(string shortname, string fullname);
    public T Invoke<T>((Type? t,object? obj) instance, string name, Type[] parametertypes, object[] parameters);
    public T InvokeStatic<T>(Type? type, string name, Type[] parametertypes, object[] parameters);
}


