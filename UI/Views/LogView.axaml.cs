using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Infrastructure.Notifications;
using System.Reflection;
using UI.ViewModels;

namespace UI;

public partial class LogView : UserControl
{
    LogViewModel vm;
    public LogView()
    {
        InitializeComponent();
        vm = new LogViewModel(FilterNotification);
        DataContext = vm;
        vm.Refilter();
    }

    /// <returns>True if the message is accepted by the filter</returns>
    public bool FilterNotification(Notification note)
    {
        if (SearchBar.Text is not null)
        {
            string trim = SearchBar.Text.Trim();

            if (!trim.Equals(string.Empty) && !note.Message.Contains(SearchBar.Text.Trim())) return false;
        }

        var tag = (TypeFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();


        if (tag != null && !tag.Equals("All"))
        {
            if (tag.Equals("Alert") && note.Priority != NotificationPriority.Alert)
                return false;
            else if (tag.Equals("Message") && note.Priority != NotificationPriority.Message)
                return false;
            else if (tag.Equals("Warning") && note.Priority != NotificationPriority.Critical)
                return false;
        }

        return true;
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        NotificationManager.OnNotified += vm.OnNotificationArrive;
    }
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        NotificationManager.OnNotified -= vm.OnNotificationArrive;
    }

    private void ClearMessages(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NotificationManager.ClearMessages();
        vm.LogEntries.Clear();
    }

    private void TypeFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    { 
        if(vm is null) return;

        vm.Refilter();
    }

    private void SearchBar_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (vm is null) return;

        vm.Refilter();
    }
}