using System.Collections.Generic;
using Avalonia.Media;
using ReactiveUI;

namespace ChatForgeUI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private ViewModelBase _content;
    public ViewModelBase Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    private Dictionary<Theme, Color> _themeColors;

    public Dictionary<Theme, Color> ThemeColors
    {
        get => _themeColors;
        set => this.RaiseAndSetIfChanged(ref _themeColors, value);
    }

    public ViewModelBase()
    {
        ThemeColors = new Dictionary<Theme, Color>
        {
            { Theme.Background, Color.Parse("#26365E") },
            { Theme.Accent, Color.Parse("#23272F") },
            { Theme.Primary, Color.Parse("#B85000") }
        };
    }

    public void SetContent(ViewModelBase content)
    {
        Content = content;
    }

    public enum Theme
    {
        Background,
        Accent,
        Primary
    }
}