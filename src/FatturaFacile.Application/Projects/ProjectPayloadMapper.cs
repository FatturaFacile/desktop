// <copyright file="ProjectPayloadMapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Maps the entities of the project portfolio to and from the JSON of the web-api (snake_case, enums as snake_case text, dates as
/// yyyy-MM-dd). Reading is tolerant: numbers may arrive as text, enums as names or numbers, dates as full timestamps.
/// </summary>
public static class ProjectPayloadMapper
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Writes a project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject ToJson(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new JsonObject
        {
            ["id"] = Id(project.Id),
            ["legal_entity_id"] = Id(project.LegalEntityId),
            ["project_code"] = project.ProjectCode,
            ["name"] = project.Name,
            ["customer_id"] = Id(project.CustomerId),
            ["project_manager_id"] = Id(project.ProjectManagerId),
            ["project_type"] = Wire(project.ProjectType),
            ["billing_model"] = Wire(project.BillingModel),
            ["cost_center_id"] = Id(project.CostCenterId),
            ["start_date"] = Date(project.StartDate),
            ["target_end_date"] = Date(project.TargetEndDate),
            ["status"] = Wire(project.Status),
            ["cup_code"] = project.CupCode,
            ["cig_code"] = project.CigCode,
            ["planned_hours"] = project.PlannedHours,
            ["actual_hours"] = project.ActualHours,
            ["planned_cost"] = project.PlannedCost,
            ["actual_cost"] = project.ActualCost,
            ["planned_revenue"] = project.PlannedRevenue,
            ["percent_complete"] = project.PercentComplete,
            ["is_template"] = project.IsTemplate,
            ["template_source_id"] = Id(project.TemplateSourceId),
        };
    }

    /// <summary>
    /// Writes a WBS node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject ToJson(WbsNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new JsonObject
        {
            ["id"] = Id(node.Id),
            ["project_id"] = Id(node.ProjectId),
            ["parent_node_id"] = Id(node.ParentNodeId),
            ["wbs_code"] = node.WbsCode,
            ["title"] = node.Title,
            ["node_type"] = Wire(node.NodeType),
            ["planned_hours"] = node.PlannedHours,
            ["actual_hours"] = node.ActualHours,
            ["planned_cost"] = node.PlannedCost,
            ["actual_cost"] = node.ActualCost,
            ["planned_revenue"] = node.PlannedRevenue,
            ["start_date"] = Date(node.StartDate),
            ["end_date"] = Date(node.EndDate),
            ["percent_complete"] = node.PercentComplete,
            ["sort_order"] = node.SortOrder,
            ["early_start"] = Date(node.EarlyStart),
            ["early_finish"] = Date(node.EarlyFinish),
            ["late_start"] = Date(node.LateStart),
            ["late_finish"] = Date(node.LateFinish),
            ["total_float_days"] = node.TotalFloatDays,
            ["free_float_days"] = node.FreeFloatDays,
            ["is_critical"] = node.IsCritical,
        };
    }

    /// <summary>
    /// Writes a dependency.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject ToJson(TaskDependency dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return new JsonObject
        {
            ["id"] = Id(dependency.Id),
            ["project_id"] = Id(dependency.ProjectId),
            ["predecessor_node_id"] = Id(dependency.PredecessorNodeId),
            ["successor_node_id"] = Id(dependency.SuccessorNodeId),
            ["dependency_type"] = Wire(dependency.DependencyType),
            ["lag_days"] = dependency.LagDays,
        };
    }

    /// <summary>
    /// Writes a baseline (the snapshot travels as a JSON object).
    /// </summary>
    /// <param name="baseline">The baseline.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject ToJson(ProjectBaseline baseline)
    {
        ArgumentNullException.ThrowIfNull(baseline);

        return new JsonObject
        {
            ["id"] = Id(baseline.Id),
            ["project_id"] = Id(baseline.ProjectId),
            ["baseline_number"] = baseline.BaselineNumber,
            ["name"] = baseline.Name,
            ["captured_at"] = DateTime.SpecifyKind(baseline.CapturedAt, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture),
            ["snapshot_json"] = TryParse(baseline.SnapshotJson),
        };
    }

    /// <summary>
    /// Reads a list of projects.
    /// </summary>
    /// <param name="json">The `data` of the answer (an array, or an object that holds one).</param>
    /// <returns>The projects that have an identifier.</returns>
    public static IReadOnlyList<Project> ParseProjects(string json) => [.. Items(json).Select(ParseProject).OfType<Project>()];

    /// <summary>
    /// Reads one project.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <returns>The project, or null when it has no identifier.</returns>
    public static Project? ParseProject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object || GuidOf(element, "id") is not { } id)
        {
            return null;
        }

        return new Project
        {
            Id = id,
            LegalEntityId = GuidOf(element, "legal_entity_id"),
            ProjectCode = Text(element, "project_code"),
            Name = Text(element, "name"),
            CustomerId = GuidOf(element, "customer_id"),
            ProjectManagerId = GuidOf(element, "project_manager_id"),
            ProjectType = EnumOf<ProjectType>(element, "project_type"),
            BillingModel = EnumOf<ProjectBillingModel>(element, "billing_model"),
            CostCenterId = GuidOf(element, "cost_center_id"),
            StartDate = DateOf(element, "start_date"),
            TargetEndDate = DateOf(element, "target_end_date"),
            Status = EnumOf<ProjectStatus>(element, "status"),
            CupCode = NullableText(element, "cup_code"),
            CigCode = NullableText(element, "cig_code"),
            PlannedHours = Number(element, "planned_hours"),
            ActualHours = Number(element, "actual_hours"),
            PlannedCost = Number(element, "planned_cost"),
            ActualCost = Number(element, "actual_cost"),
            PlannedRevenue = Number(element, "planned_revenue"),
            PercentComplete = Number(element, "percent_complete"),
            IsTemplate = Flag(element, "is_template"),
            TemplateSourceId = GuidOf(element, "template_source_id"),
        };
    }

    /// <summary>
    /// Reads the WBS nodes of a project.
    /// </summary>
    /// <param name="json">The `data` of the answer.</param>
    /// <param name="projectId">The project the nodes belong to when the payload does not say.</param>
    /// <returns>The nodes that have an identifier.</returns>
    public static IReadOnlyList<WbsNode> ParseNodes(string json, Guid projectId) =>
        [.. Items(json).Select(element => ParseNode(element, projectId)).OfType<WbsNode>()];

    /// <summary>
    /// Reads the dependencies of a project.
    /// </summary>
    /// <param name="json">The `data` of the answer.</param>
    /// <param name="projectId">The project the dependencies belong to when the payload does not say.</param>
    /// <returns>The dependencies that name both nodes.</returns>
    public static IReadOnlyList<TaskDependency> ParseDependencies(string json, Guid projectId) =>
        [.. Items(json).Select(element => ParseDependency(element, projectId)).OfType<TaskDependency>()];

    /// <summary>
    /// Reads the baselines of a project.
    /// </summary>
    /// <param name="json">The `data` of the answer.</param>
    /// <param name="projectId">The project the baselines belong to when the payload does not say.</param>
    /// <returns>The baselines that have an identifier.</returns>
    public static IReadOnlyList<ProjectBaseline> ParseBaselines(string json, Guid projectId) =>
        [.. Items(json).Select(element => ParseBaseline(element, projectId)).OfType<ProjectBaseline>()];

    private static WbsNode? ParseNode(JsonElement element, Guid projectId)
    {
        if (element.ValueKind != JsonValueKind.Object || GuidOf(element, "id") is not { } id)
        {
            return null;
        }

        return new WbsNode
        {
            Id = id,
            ProjectId = GuidOf(element, "project_id") ?? projectId,
            ParentNodeId = GuidOf(element, "parent_node_id"),
            WbsCode = Text(element, "wbs_code"),
            Title = Text(element, "title"),
            NodeType = EnumOf<WbsNodeType>(element, "node_type"),
            PlannedHours = Number(element, "planned_hours"),
            ActualHours = Number(element, "actual_hours"),
            PlannedCost = Number(element, "planned_cost"),
            ActualCost = Number(element, "actual_cost"),
            PlannedRevenue = Number(element, "planned_revenue"),
            StartDate = DateOf(element, "start_date"),
            EndDate = DateOf(element, "end_date"),
            PercentComplete = Number(element, "percent_complete"),
            SortOrder = (int)Number(element, "sort_order"),
            EarlyStart = DateOf(element, "early_start"),
            EarlyFinish = DateOf(element, "early_finish"),
            LateStart = DateOf(element, "late_start"),
            LateFinish = DateOf(element, "late_finish"),
            TotalFloatDays = IntOf(element, "total_float_days"),
            FreeFloatDays = IntOf(element, "free_float_days"),
            IsCritical = Flag(element, "is_critical"),
        };
    }

    private static TaskDependency? ParseDependency(JsonElement element, Guid projectId)
    {
        if (element.ValueKind != JsonValueKind.Object
            || GuidOf(element, "id") is not { } id
            || GuidOf(element, "predecessor_node_id") is not { } predecessor
            || GuidOf(element, "successor_node_id") is not { } successor)
        {
            return null;
        }

        return new TaskDependency
        {
            Id = id,
            ProjectId = GuidOf(element, "project_id") ?? projectId,
            PredecessorNodeId = predecessor,
            SuccessorNodeId = successor,
            DependencyType = EnumOf<DependencyType>(element, "dependency_type"),
            LagDays = IntOf(element, "lag_days") ?? 0,
        };
    }

    private static ProjectBaseline? ParseBaseline(JsonElement element, Guid projectId)
    {
        if (element.ValueKind != JsonValueKind.Object || GuidOf(element, "id") is not { } id)
        {
            return null;
        }

        var snapshot = element.TryGetProperty("snapshot_json", out var raw) && raw.ValueKind != JsonValueKind.Null
            ? raw.ValueKind == JsonValueKind.String ? raw.GetString() ?? "{}" : raw.GetRawText()
            : "{}";

        return new ProjectBaseline
        {
            Id = id,
            ProjectId = GuidOf(element, "project_id") ?? projectId,
            BaselineNumber = IntOf(element, "baseline_number") ?? 0,
            Name = Text(element, "name"),
            CapturedAt = InstantOf(element, "captured_at"),
            SnapshotJson = snapshot,
        };
    }

    private static IEnumerable<JsonElement> Items(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Object)
        {
            root = root.TryGetProperty("data", out var data) ? data : root;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            root = root.TryGetProperty("items", out var items) ? items : root;
        }

        return root.ValueKind == JsonValueKind.Array ? [.. root.EnumerateArray().Select(item => item.Clone())] : [];
    }

    private static JsonNode? TryParse(string json)
    {
        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Id(Guid? id) => id?.ToString("D", CultureInfo.InvariantCulture);

    private static string Id(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);

    private static string? Date(DateOnly? date) => date?.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string Wire<T>(T value)
        where T : struct, Enum
    {
        var name = value.ToString();
        var text = new System.Text.StringBuilder(name.Length + 4);
        for (var index = 0; index < name.Length; index++)
        {
            var current = name[index];
            if (index > 0 && char.IsUpper(current))
            {
                var previous = name[index - 1];
                var next = index + 1 < name.Length ? name[index + 1] : 'A';
                if (!char.IsUpper(previous) || char.IsLower(next))
                {
                    _ = text.Append('_');
                }
            }

            _ = text.Append(char.ToLowerInvariant(current));
        }

        return text.ToString();
    }

    private static T EnumOf<T>(JsonElement element, string name)
        where T : struct, Enum
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return default;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && Enum.IsDefined(typeof(T), number))
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            return default;
        }

        var wanted = Normalize(value.GetString() ?? string.Empty);
        foreach (var candidate in Enum.GetValues<T>())
        {
            if (string.Equals(Normalize(candidate.ToString()), wanted, StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return default;
    }

    private static string Normalize(string text) => text.Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string Text(JsonElement element, string name) => NullableText(element, name) ?? string.Empty;

    private static string? NullableText(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static Guid? GuidOf(JsonElement element, string name) =>
        Guid.TryParse(NullableText(element, name), out var id) && id != Guid.Empty ? id : null;

    private static decimal Number(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return 0m;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0m,
        };
    }

    private static int? IntOf(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.Number or JsonValueKind.String
            ? (int)Math.Round(Number(element, name), MidpointRounding.AwayFromZero)
            : null;

    private static bool Flag(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
        && (value.ValueKind == JsonValueKind.True || (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number != 0));

    private static DateOnly? DateOf(JsonElement element, string name)
    {
        var text = NullableText(element, name);
        if (text is null || text.Length < DateFormat.Length)
        {
            return null;
        }

        return DateOnly.TryParseExact(text[..DateFormat.Length], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }

    private static DateTime InstantOf(JsonElement element, string name) =>
        DateTime.TryParse(NullableText(element, name), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var instant)
            ? DateTime.SpecifyKind(instant, DateTimeKind.Utc)
            : DateTime.UnixEpoch;
}
