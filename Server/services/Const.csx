using static System.Exception;
using Grinder;

public class Const:IService 
{
    public string uid{ get=>"Const";}
    public int Container=0;
    public void OnEvent(object e){}
}

public Exception Instance2(out IService service/*,  object x*/)
{
    service=new Const();
    return null;
}

return Instance2( out t/*,x*/);