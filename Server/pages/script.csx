using static System.Exception;
using Grinder;

public class Page:IPage
{
    public int Id { get; set; }

    public Page(int Id){
        this.Id=Id;
    }
    public string OnInitialize(string Url, Dictionary<string, string> parameters){
        return "test";
    }

}

public Exception Start(int id,out IPage page){
    page=new Page(id);
    return null;
}

return Start(id,out page);
