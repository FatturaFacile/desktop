using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.ViewModels;

/// <summary>
    /// Represents view model base.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject
    {
    /// <summary>
    /// Gets or sets a value indicating whether busy.
    /// </summary>
[ObservableProperty]
    public partial bool IsBusy { get; set; }

/// <summary>
    /// Gets or sets the error message.
    /// </summary>
[ObservableProperty]
    public partial string? ErrorMessage { get; set; }

/// <summary>
/// Initialize Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public virtual Task InitializeAsync() => Task.CompletedTask;
}
