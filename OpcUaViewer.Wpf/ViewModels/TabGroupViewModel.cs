using System.Collections.Generic;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.ViewModels;

public class TabGroupViewModel : ViewModelBase
{
    private IAppTab  _selectedTab;
    private readonly Dictionary<IAppTab, FrameworkElement> _viewCache = [];

    public string          Key        { get; }
    public string          Icon       => Tabs[0].Icon;
    public string          Title      => Tabs[0].Title;
    public bool            PinToBottom => Tabs[0].PinToBottom;
    public List<IAppTab>   Tabs       { get; }
    public bool            HasMultipleTabs => Tabs.Count > 1;

    public IAppTab SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (Set(ref _selectedTab, value))
                Notify(nameof(SelectedView));
        }
    }

    public FrameworkElement? SelectedView
    {
        get
        {
            if (!_viewCache.TryGetValue(_selectedTab, out var view))
                _viewCache[_selectedTab] = view = _selectedTab.CreateView();
            return view;
        }
    }

    public RelayCommand<IAppTab> SelectTabCommand { get; }

    public TabGroupViewModel(string key, List<IAppTab> tabs)
    {
        Key            = key;
        Tabs           = tabs;
        _selectedTab   = tabs[0];
        SelectTabCommand = new RelayCommand<IAppTab>(t => { if (t is not null) SelectedTab = t; });
    }
}
