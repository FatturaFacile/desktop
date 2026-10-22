using System.Collections.ObjectModel;

namespace FatturaFacile.Model;

public class KanbanGroup(string caption)
{
    public string Caption { get; set; } = caption;
    public ObservableCollection<KanbanItem> Items { get; set; }
}

public class KanbanItem(string caption, string description, bool isComplete)
{
    public KanbanItem() : this(null, null, false) { }
    public string Caption { get; set; } = caption;
    public string Description { get; set; } = description;
    public bool IsComplete { get; set; } = isComplete;
}