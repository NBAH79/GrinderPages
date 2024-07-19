using static System.Exception;
using System.Reflection;
using Grinder;
using System.Diagnostics;

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

    Stopwatch sw=new Stopwatch();
sw.Start();
    var x=manager.Instance("UserLibrary","Static.Const");
    id=manager.Invoke<int>(x,"GetModelEnumerator",new Type[] { typeof(string) },new object[]{"Container"});
sw.Stop();
Console.WriteLine(sw.ElapsedTicks);
sw.Restart();
    var y=manager.Static("UserLibrary","Static.Const_");
    Console.WriteLine(y.ToString());
sw.Stop();
Console.WriteLine(sw.ElapsedTicks);
Console.WriteLine(Stopwatch.Frequency);
    id=manager.InvokeStatic<int>(y,"GetModelEnumerator",new Type[] { typeof(string) },new object[]{"Container"});

    Console.WriteLine(id.ToString());
    ret= new Container(id);

}

Instance(manager, out ret);