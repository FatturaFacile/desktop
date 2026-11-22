// <copyright file="ProjectValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The field rules of projects, WBS nodes and dependencies. A rule that fails returns a localization key.
/// </summary>
public static class ProjectValidator
{
    /// <summary>
    /// The maximum lag or lead in days.
    /// </summary>
    public const int MaxLagDays = 365;

    /// <summary>
    /// The length of a CUP code.
    /// </summary>
    public const int CupLength = 15;

    /// <summary>
    /// The length of a CIG code.
    /// </summary>
    public const int CigLength = 10;

    /// <summary>
    /// The key of a missing or too long project code.
    /// </summary>
    public const string CodeKey = "Project_Err_Code";

    /// <summary>
    /// The key of a missing or too long project name.
    /// </summary>
    public const string NameKey = "Project_Err_Name";

    /// <summary>
    /// The key of an end before the start.
    /// </summary>
    public const string DatesKey = "Project_Err_Dates";

    /// <summary>
    /// The key of a malformed CUP code.
    /// </summary>
    public const string CupKey = "Project_Err_Cup";

    /// <summary>
    /// The key of a malformed CIG code.
    /// </summary>
    public const string CigKey = "Project_Err_Cig";

    /// <summary>
    /// The key of a code that another project uses.
    /// </summary>
    public const string DuplicateCodeKey = "Project_Err_DuplicateCode";

    /// <summary>
    /// The key of an unknown project, template or record.
    /// </summary>
    public const string NotFoundKey = "Project_Err_NotFound";

    /// <summary>
    /// The key of a missing or too long node title.
    /// </summary>
    public const string TitleKey = "Project_Err_Title";

    /// <summary>
    /// The key of a progress outside 0-100.
    /// </summary>
    public const string PercentKey = "Project_Err_Percent";

    /// <summary>
    /// The key of a negative amount.
    /// </summary>
    public const string AmountKey = "Project_Err_Amount";

    /// <summary>
    /// The key of an invalid parent.
    /// </summary>
    public const string ParentKey = "Project_Err_Parent";

    /// <summary>
    /// The key of a node that cannot become a summary because it has dependencies.
    /// </summary>
    public const string SummaryLinkKey = "Project_Err_SummaryLink";

    /// <summary>
    /// The key of a dependency that is not valid.
    /// </summary>
    public const string DependencyKey = "Project_Err_Dependency";

    /// <summary>
    /// The key of a dependency that closes a cycle.
    /// </summary>
    public const string CycleKey = "Project_Err_Cycle";

    /// <summary>
    /// The key of a lag outside the allowed range.
    /// </summary>
    public const string LagKey = "Project_Err_Lag";

    /// <summary>
    /// The key of a too long baseline name.
    /// </summary>
    public const string BaselineNameKey = "Project_Err_BaselineName";

    /// <summary>
    /// Checks the fields of a project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns>The key of the first failed rule, or null.</returns>
    public static string? ValidateProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (string.IsNullOrWhiteSpace(project.ProjectCode) || project.ProjectCode.Trim().Length > 64)
        {
            return CodeKey;
        }

        if (string.IsNullOrWhiteSpace(project.Name) || project.Name.Trim().Length > 200)
        {
            return NameKey;
        }

        if (project.StartDate is { } start && project.TargetEndDate is { } end && end < start)
        {
            return DatesKey;
        }

        if (!IsCode(project.CupCode, CupLength))
        {
            return CupKey;
        }

        return IsCode(project.CigCode, CigLength) ? null : CigKey;
    }

    /// <summary>
    /// Checks the fields of a WBS node (the structure is checked against the tree by the service).
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The key of the first failed rule, or null.</returns>
    public static string? ValidateNode(WbsNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (string.IsNullOrWhiteSpace(node.Title) || node.Title.Trim().Length > 300)
        {
            return TitleKey;
        }

        if (node.WbsCode is { Length: > 64 })
        {
            return CodeKey;
        }

        if (node.StartDate is { } start && node.EndDate is { } end && end < start)
        {
            return DatesKey;
        }

        if (node.PercentComplete is < 0m or > 100m)
        {
            return PercentKey;
        }

        return node.PlannedHours < 0m || node.ActualHours < 0m || node.PlannedCost < 0m || node.ActualCost < 0m || node.PlannedRevenue < 0m
            ? AmountKey
            : null;
    }

    /// <summary>
    /// Checks the lag of a dependency.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>The key of the failed rule, or null.</returns>
    public static string? ValidateDependency(TaskDependency dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (dependency.PredecessorNodeId == Guid.Empty || dependency.SuccessorNodeId == Guid.Empty || dependency.PredecessorNodeId == dependency.SuccessorNodeId)
        {
            return DependencyKey;
        }

        return Math.Abs(dependency.LagDays) > MaxLagDays || !Enum.IsDefined(dependency.DependencyType) ? LagKey : null;
    }

    private static bool IsCode(string? code, int length) =>
        string.IsNullOrWhiteSpace(code) || (code.Trim().Length == length && code.Trim().All(char.IsAsciiLetterOrDigit));
}
