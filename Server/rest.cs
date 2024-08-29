using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Rest;


public enum Status:int {
    offline = 0,
    register = 1,
    unregister = 2,
    update = 3,
}

public class Node
{
    public string key {get;set;} = ""; //безопасность
    public Status status {get;set;} =0;
    public int id {get;set;} =0;
    public Guid uid {get;set;} = Guid.Empty;
    public string YYY {get;set;}= string.Empty;
    public int Sessions {get;set;}= 0;
    public int priority {get;set;}= 1000; //процент балансировки
}

public class NodeResponse
{
    public int id {get;set;} = 0;
}
