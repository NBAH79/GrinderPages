using static System.Exception;
using System.Reflection;
using Grinder;

public class Container : IModel
{
    public int _id{ get; set; }
    public string Id { get; set; } = "";
    public string Tag { get; set; } = "div";
    public string Class { get; set; } = "container";
    public string Style { get; set; } = "";

    public Container(int id) { _id = id;}
    public void OnEvent(object e) { }
}

public void Instance(IManager manager, out IModel ret)
{
    int id=0;
    Console.WriteLine("a");
    var a=manager.GetAssembly("../../../dlls/UserLibrary.dll");
    Console.WriteLine(a.ToString());
    Type? t= manager.GetClass(a,"Static.Const");
    Console.WriteLine(t.ToString());
    ConstructorInfo? constructor = t.GetConstructor(Type.EmptyTypes);
    object? obj = constructor?.Invoke(new object[] { });
    //var m=manager.GetMethod(t, "GetModelEnumerator", new Type[] { typeof(string), typeof(int).MakeByRefType() });
    var m=manager.GetMethod(t, "GetModelEnumerator", new Type[] { typeof(string) });
    object x=m?.Invoke(obj,new object[]{"Container"});
    Console.WriteLine(x.ToString());
    ret= new Container((int)x);
    //Console.WriteLine(t.ToString());
    //t=c;
    //return new Container(id);
    //model.Style=x as string;
    //return null;
}

Instance(manager, out ret);