using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private IAppTab? _selectedTab;
    private readonly Dictionary<IAppTab, FrameworkElement> _viewCache = [];

    public ObservableCollection<IAppTab> NavTabs    { get; } = [];
    public ObservableCollection<IAppTab> PinnedTabs { get; } = [];

    public IAppTab? SelectedTab
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
            if (_selectedTab is null) return null;
            if (!_viewCache.TryGetValue(_selectedTab, out var view))
                _viewCache[_selectedTab] = view = _selectedTab.CreateView();
            return view;
        }
    }

    public RelayCommand<IAppTab> SelectTabCommand { get; }

    public MainViewModel(IEnumerable<IAppTab> tabs)
    {
        SelectTabCommand = new RelayCommand<IAppTab>(tab => SelectedTab = tab);

        foreach (var tab in tabs.OrderBy(t => t.Order))
        {
            if (tab.PinToBottom) PinnedTabs.Add(tab);
            else                 NavTabs.Add(tab);
        }

        SelectedTab = NavTabs.FirstOrDefault();
    }
}
