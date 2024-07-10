namespace Grinder;

public interface IController
{
    public string OnInitialize(IController? parent = null);
    public string OnRelease();

    public string OnCreate(Stream stream, IController? parent = null);
    public string OnDestroy(Stream stream, int msec);

    public string OnPreRender(Stream stream, bool refresh, IController? parent = null);
    public string OnRender(Stream stream, bool refresh, IController? parent = null);
    public string OnAfterRender(Stream stream, bool refresh, IController? parent = null);

    public string OnEvent(Stream stream, string[] operands);
    public string OnUpdate();
    public string OnTimer();
}