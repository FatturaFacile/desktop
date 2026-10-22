using System;
using System.Collections.Generic;

namespace FatturaFacile.Model;

public class GanttTaskItem(int id, int? parentId, DateTime startDate, DateTime finishDate, string name, double progress, IEnumerable<GanttTaskLink> links)
{
    public int Id { get; set; } = id;
    public int? ParentId { get; set; } = parentId;
    public DateTime StartDate { get; set; } = startDate;
    public DateTime FinishDate { get; set; } = finishDate;
    public string Name { get; set; } = name;
    public double Progress { get; set; } = progress;
    public IEnumerable<GanttTaskLink> Links { get; set; } = links;
}

public class GanttTaskLink
{
    public GanttTaskLink(int predecessorId)
    {
        PredecessorId = predecessorId;
    }
    public int PredecessorId { get; private set; }
}