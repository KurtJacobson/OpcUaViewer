using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private TabGroupViewModel? _selectedGroup;

    public ObservableCollection<TabGroupViewModel> NavGroups    { get; } = [];
    public ObservableCollection<TabGroupViewModel> PinnedGroups { get; } = [];

    public TabGroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set => Set(ref _selectedGroup, value);
    }

    public RelayCommand<TabGroupViewModel> SelectGroupCommand { get; }

    public MainViewModel(IEnumerable<IAppTab> tabs)
    {
        SelectGroupCommand = new RelayCommand<TabGroupViewModel>(g => SelectedGroup = g);

        var groups = tabs
            .OrderBy(t => t.Order)
            .GroupBy(t => t.GroupKey)
            .Select(g => new TabGroupViewModel(g.Key, g.ToList()));

        foreach (var group in groups)
        {
            if (group.PinToBottom) PinnedGroups.Add(group);
            else                   NavGroups.Add(group);
        }

        SelectedGroup = NavGroups.FirstOrDefault();
    }
}
