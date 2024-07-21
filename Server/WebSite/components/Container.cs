using System.Reflection;
using Grinder;
using System.Diagnostics;

public void Instance(IManager manager, out Model t)
{
    int id=0;
    Console.WriteLine("a");

    Stopwatch sw=new Stopwatch();
sw.Start();
    var x=manager.GetInstance("UserLibrary","Static.Const");
    id=manager.Invoke<int>(x,"GetModelEnumerator",new Type[] { typeof(string) },new object[]{"Container"});
sw.Stop();
Console.WriteLine(sw.ElapsedTicks);
sw.Restart();
    var y=manager.GetStatic("UserLibrary","Static.Const_");
    Console.WriteLine(y.ToString());
sw.Stop();
Console.WriteLine(sw.ElapsedTicks);
Console.WriteLine(Stopwatch.Frequency);
    id=manager.InvokeStatic<int>(y,"GetModelEnumerator",new Type[] { typeof(string) },new object[]{"Container"});

    Console.WriteLine(id.ToString());
    t= new Model("","div","container","",id);

}

Instance(manager, out t);