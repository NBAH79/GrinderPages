namespace Grinder;

public abstract class Page
{
    public string _id { get; }
    public List<Component> body = new List<Component>();
    public Page(List<Component> body) { this.body = body; }

    public async Task Create(Stream stream, string Url, Dictionary<string, string> parameters)
    {
        await OnCreate(stream, Url, parameters);
        foreach (var x in body) await x.Create(stream);
        await Task.CompletedTask;
    }
    public async Task Render(Stream stream, string Url, bool refresh, Dictionary<string, string> parameters)
    {
        await OnRender(stream, Url, refresh, parameters);
        //PageNotFoundText.Content = $"Страница <span class='red'>{Url}</span> не найдена!";
        foreach (var x in body) await x.Render(stream, refresh);
        await Task.CompletedTask;
    }

    public async Task Event(Stream stream, string[] operands)
    {
        await OnEvent(stream, operands);
        foreach (var x in body) await x.Event(stream, operands);
        await Task.CompletedTask;
    }

    //public async Task Update()
    //{
    //    await OnUpdate();
    //    foreach (var x in body) await x.Update();
    //    await Task.CompletedTask;
    //}
    public virtual Task OnCreate(Stream stream, string Url, Dictionary<string, string> parameters) { return Task.CompletedTask; }
    public virtual Task OnRender(Stream stream, string Url, bool refresh, Dictionary<string, string> parameters) { return Task.CompletedTask; }

    public virtual Task OnEvent(Stream stream, string[] operands) { return Task.CompletedTask; }

    public virtual Task OnUpdate() { return Task.CompletedTask; }
}
