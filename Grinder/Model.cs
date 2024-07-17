using System;

namespace Grinder;

public interface IModel
{
    public int _id{get;set;}
    public string Id {get;set;}
    public string Tag {get;set;}
    public string Class {get;set;}
    public string Style {get;set;}

    
    void OnEvent(object e);
}
