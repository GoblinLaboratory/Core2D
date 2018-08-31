using Avalonia;
using Avalonia.Markup.Xaml;

namespace Core2D.UI.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
