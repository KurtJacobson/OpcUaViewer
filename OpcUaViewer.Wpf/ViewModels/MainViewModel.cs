using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private IAppTab? _selectedTab;

    public ObservableCollection<IAppTab> NavTabs    { get; } = [];
    public ObservableCollection<IAppTab> PinnedTabs { get; } = [];

    public IAppTab? SelectedTab
    {
        get => _selectedTab;
        set => Set(ref _selectedTab, value);
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
