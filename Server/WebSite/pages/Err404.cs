using Grinder;

public class Frame : Component
{

    public Frame(IManager manager,string Class, string Style, List<Component> children):base(manager,children)
    {
        Tag = "div";
        this.Class = "frame " + Class;
        this.Style = Style;
    }
    public override async Task OnEvent(Stream stream, string[] operands) { await Task.CompletedTask; }
    public override async Task OnUpdate() { await Task.CompletedTask; }
    public override void OnRelease() { }
}

public class Block : Component
{

    public Block(IManager manager,string Class, string Style, List<Component> children) : base(manager,children)
    {
        Tag = "div";
        this.Class = Class;
        this.Style = Style;
    }
    public override async Task OnEvent(Stream stream, string[] operands) { await Task.CompletedTask; }
    public override async Task OnUpdate() { await Task.CompletedTask; }
    public override void OnRelease() { }
}
public class Text : Component
{

    public Text(IManager manager,string Class, string Style, string text):base(manager)
    {
        Tag = "p";
        this.Class = Class;
        this.Style = Style;
        this.Content = text;
    }
    public override async Task OnEvent(Stream stream, string[] operands) { await Task.CompletedTask; }
    public override async Task OnUpdate() { await Task.CompletedTask; }
    public override void OnRelease() { }
    public void SetText(string newtext)
    {
        Content = newtext;
        hash = 0;
    }
}

public class Button : Component
{
    public string height { get; set; } = "48px";
    public string opacity { get; set; } = "1";
    public string title { get; set; } = "BUTTON";

    //public string left{ get; set; } = "0";
    public string style { get; set; } = "position:relative;margin:56px auto 0";

    public int position = 0;
    public EventResult onclick { get; set; } = (async (__s, __o) => { return await Task.FromResult("UPD|"); });

    public Button(IManager manager):base(manager){
        Events.Clear();
        Events.Add("click:3");
    }

    public override async Task OnUpdate()
    {
        //Tag = "div";
        //Class = "wb";
        position += 50;
        //left = $"{position}px";
        if (position > 300) position = 0;
        Style = $"{style};left:{position}px;transition: all 2s ease-out 0s;width:200px;height:{height};opacity:{opacity}";
        //Events.Clear();
        //Events.Add("click:3");//,$"sendText('{this.id}onclick')");
        hash = 0;
        await Task.CompletedTask;
        //Content="title";
    }

    public override void OnRelease()
    {
        height = "0";
        opacity = "0";
    }

    //public override string OnRender()
    //{
    //    return $"<button ref='{id}' style='transition: all 2s ease-out 0s;width:200px;height:{height}'>{title}</button>";
    //    //return base.OnRender(operands);
    //}

    public override async Task OnEvent(Stream stream, string[] operands)
    {
        if (operands[0] == Id && operands[1] == "click") await manager.SendText(stream, await onclick(stream, operands));
    }
}
// public class Input : Component
// {


//     public override async Task OnEvent(Stream stream, string[] operands) { await Task.CompletedTask; }
//     public override async Task OnUpdate() { await Task.CompletedTask; }
//     public override void OnRelease() { }

//     public string type { get; set; } = "text";
//     public string placeholder { get; set; } = "SOME TEXT";

//     //public override string OnRender()
//     //{
//     //    return $"<input ref='{id}' style='width:200px;height:48px' type='{type}' placeholder='{placeholder}'><label>INPUT</label></input>";
//     //    //return base.OnRender(operands);
//     //}
// }

public class Err404Page : Page
{
    public override string _id { get => "Err404.html"; }

    public Text PageNotFoundText;// = new Text(manager,"t14 cg87", "", "");// = new Text("t14 cg87", "", "Страница не найдена!");
    //public Block BlockWithText;

    
}

public void Instance(IManager manager, out Page t)
{
    // var x=manager.GetStatic("UserLibrary","Static.Const_");
    // var id=manager.InvokeStatic<int>(x,"GetModelEnumerator",new Type[] { typeof(string) },new object[]{"Container"});
    // var model=manager.GetModel(id);

    //Console.WriteLine(id.ToString());
    //Console.WriteLine(model?.ToString());
    
    var ret = new Err404Page();
    //ret.BlockWithText=new Block(manager,"","",new List<Component>());
    //ret.PageNotFoundText=new Text(manager,"t14 cg87", "", "");

    ret.body = new List<Component>(){
            new Frame(manager,"_white", "margin:24px;padding:12px;height:100%;width:auto",new List<Component>() {
                new Block(manager,"col", "margin:auto", new List<Component>() {
                    new Text(manager,"t24 red","margin-bottom:8px; filter: drop-shadow(1px 1px 1px #FFFFF0) drop-shadow(-1px -1px 1px #A0A0AF)","Error404"),
                    { ret.PageNotFoundText = new Text(manager,"t14 cg87", "", "Страница не найдена!") },
                    new Button(manager){
                        Class="rb",
                        style="margin:56px auto 0",
                        height="40px",
                        Content="<a class='bcs white' style='width:100%;height:100%;line-height:38px;'>НА ГЛАВНУЮ</a>",
                        //onclick=(async (__s,__o)=>{return await Task.FromResult($"URL|{manager.globalWWW}Index.html"); })
                        onclick=(async (__s,__o)=>{return await Task.FromResult($"URL|Index.html"); })
                        }
                    })
                })

            };
    t=ret;
}

Instance(manager, out t);
