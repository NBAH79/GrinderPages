using System.Reflection;

namespace Grinder;

public interface IManager
{
    Assembly GetAssembly(string filename);
    Type? GetClass(Assembly assembly,string fullname); //лучше для оптимизации
    MethodInfo? GetMethod(Type type,string name, Type[] parameters);
    //MethodInfo GetMethod(string assemblyname, string fulltypename,string methodname); //компактный
}
