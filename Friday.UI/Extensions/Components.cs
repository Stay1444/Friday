using DSharpPlus;
using Friday.UI.Components;
using Friday.UI.Entities;

namespace Friday.UI.Extensions;

public static class Components
{
    public static void AddButton(this FridayUIPage page, Action<ButtonComponent> modify)
    { 
        var component = new ButtonComponent(page);
        modify(component);
        page.Add(component);
    }

    public static async Task AddButton(this FridayUIPage page, Func<ButtonComponent, Task> modifyAsync)
    {
        var component = new ButtonComponent(page);
        await modifyAsync(component);
        page.Add(component);
    }
    
    public static void AddModal(this FridayUIPage page, Action<ModalComponent> modify)
    {
        var component = new ModalComponent(page);
        modify(component);
        page.Add(component);
        
    }
    
    public static void AddSelect(this FridayUIPage page, Action<SelectComponent> modify)
    {
        var component = new SelectComponent(page);
        modify(component);
        page.Add(component);
    }
    
    public static async Task AddSelect(this FridayUIPage page, Func<SelectComponent, Task> modifyAsync)
    {
        var component = new SelectComponent(page);
        await modifyAsync(component);
        page.Add(component);
    }
}