namespace Grinder;

public interface IService
{
    public string uid {get;}
    void OnEvent(object e);

}

