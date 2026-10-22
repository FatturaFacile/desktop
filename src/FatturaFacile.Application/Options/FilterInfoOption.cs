// <copyright file="FilterInfoOption.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Configuration;

public sealed class FilterInfoOption
{
    public string Name { get; init; } = string.Empty;
    public string FilterCriteria { get; init; } = string.Empty;
    public string ImageUri { get; init; } = string.Empty;
}

public sealed class DevAVFilterOptions
{
    public const string SectionName = "DevAVFilters";

    public List<FilterInfoOption> TasksCustomFilters { get; init; } = [];
    public List<FilterInfoOption> EmployeesCustomFilters { get; init; } = [];
    public List<FilterInfoOption> CustomersCustomFilters { get; init; } = [];
    public List<FilterInfoOption> CustomersStaticFilters { get; init; } = [];
    public List<FilterInfoOption> ProductsCustomFilters { get; init; } = [];
    public List<FilterInfoOption> ProductsStaticFilters { get; init; } = [];
    public List<FilterInfoOption> OrdersCustomFilters { get; init; } = [];
    public List<FilterInfoOption> OrdersStaticFilters { get; init; } = [];
    public List<FilterInfoOption> QuotesStaticFilters { get; init; } = [];
    public List<FilterInfoOption> EmployeesStaticFilters { get; init; } = [];
    public List<FilterInfoOption> TasksStaticFilters { get; init; } = [];
}

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public AzureOpenAiOptions AzureOpenAI { get; init; } = new();
    public OllamaOptions Ollama { get; init; } = new();
    public OpenAiOptions OpenAI { get; init; } = new();
}

public sealed class AzureOpenAiOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string Deployment { get; init; } = string.Empty;
    public string EmbeddingsDeployment { get; init; } = string.Empty;
}

public sealed class OllamaOptions
{
    public string Uri { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}

public sealed class OpenAiOptions
{
    public string Key { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
