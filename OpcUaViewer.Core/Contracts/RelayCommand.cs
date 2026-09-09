using System;
using System.Windows.Input;

namespace OpcUaViewer.Core.Contracts;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? _) => canExecute?.Invoke() ?? true;
    public void Execute(object? _)    => execute();
}

public sealed class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? p) => canExecute?.Invoke((T?)p) ?? true;
    public void Execute(object? p)    => execute((T?)p);
}
