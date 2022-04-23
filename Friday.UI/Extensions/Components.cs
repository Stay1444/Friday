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
    
    public static void AddModal(this FridayUIPage page, Action<ModalComponent> modify)
    {
        var component = new ModalComponent(page);
        modify(component);
        page.Add(component);
    }
    
}