using static System.Exception;
using Grinder;

public class Container:IModel
{
    public int Uid {get;}
    public string Id {get;set;} = "";
    public string Tag {get;set;} = "div";
    public string Class {get;set;} = "container";
    public string Style {get;set;} = "";
    public Container(int uid){Uid=uid;}
}

public Exception Instance(int uid,out IModel model, object x){

    model=new Container(uid);
    model.Style=x as string;
    return null;
}

return Instance(uid, out model,x);