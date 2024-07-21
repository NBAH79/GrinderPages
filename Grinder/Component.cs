namespace Grinder;

public abstract class Component
{
    public List<Component> children = new List<Component>();
    public string Id = string.Empty;
    public string Tag = string.Empty;
    public string Class = string.Empty;
    public string Style = string.Empty;
    public string Content = string.Empty;
    public Dictionary<string, string> Attributes = new Dictionary<string, string>();
    public List<string> Events = new List<string>();

    //public abstract Task SendText(Stream stream, string text);

    protected int hash = 0;

    private IManager manager;

    public Component(IManager manager)
    {
        this.manager = manager;
        Id = Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
        children = new List<Component>();
    }

    //конструктор шаблона
    public Component(IManager manager, List<Component> template)
    {
        this.manager = manager;
        Id = Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
        children = new List<Component>();
        foreach (var x in template) this.children.Add(x.Instantiate()); //инстанс
    }
    public Component Instantiate()
    {
        var ret = this.MemberwiseClone();
        return ret as Component;
    }

    public async Task OnCreate(Stream stream, Component? parent = null)
    {
        await OnUpdate();
        string res = $"|{Id}|{parent?.Id}|{Tag}|{Class}|{Style}|";
        foreach (var a in Attributes) res += $"{a.Key}={a.Value};";
        res += "|";
        foreach (var e in Events) res += $"{e};";
        res += $"|{Content}";
        hash = res.GetHashCode();
        await manager.SendText(stream, res);
    }

    //просто тупая отправка
    //надо чтоб возвращало просто строку
    public async Task OnRender(Stream stream, bool refresh, Component? parent = null)
    {
        await OnUpdate();
        string res = $"|{Id}|{parent?.Id}|{Tag}|{Class}|{Style}|";
        foreach (var a in Attributes) res += $"{a.Key}={a.Value};";
        res += "|";
        //евенты не обновлять, даже лучше их отдельно сделать
        //if (refresh) foreach (var e in Events) res += $"{e};";
        res += $"|{Content}";
        int newhash = res.GetHashCode();
        //if (newhash == hash && !refresh) return;
        await manager.SendText(stream, res);
        hash = newhash;
    }

    //Create и Render зашиты, а эти непонятные переопеределяются
    public abstract Task OnEvent(Stream stream, string[] operands);// { await Task.CompletedTask; }
    public abstract Task OnUpdate();// { await Task.CompletedTask; }
    public abstract void OnRelease();// { }
    public async Task Create(Stream stream, Component? parent = null)
    {
        await OnCreate(stream, parent);
        foreach (var x in children) await x.Create(stream, this);
    }

    public async Task Render(Stream stream, bool refresh, Component? parent = null)
    {
        await OnRender(stream, refresh, parent);
        foreach (var c in children) await c.Render(stream, refresh, this);
    }

    public async Task Event(Stream stream, string[] operands)
    {
        await OnEvent(stream, operands);
        foreach (var x in children) await x.Event(stream, operands);
    }

    //если есть параметры то это должно сработать до render
    //public async Task Update()
    //{
    //    await OnUpdate();
    //    foreach (var x in children) await x.Update();
    //}

    //с задержкой дает возможность прокрутить анимацию
    public async Task Destroy(Stream stream, int msec)
    {
        OnRelease();
        await OnUpdate();
        await Render(stream, true);
        await manager.SendText(stream, $"{Id}|{msec}");
        //_=Task.Factory.StartNew(async () =>
        //{
        //      await Task.Delay(msec);
        //      await Node.SendText(stream, $"{id}|");
        //});
    }

}
