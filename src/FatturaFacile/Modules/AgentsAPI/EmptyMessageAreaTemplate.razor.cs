using Microsoft.AspNetCore.Components;

namespace AIDemo {
    public partial class EmptyMessageAreaTemplate : ComponentBase {
        [Parameter] public string Icon { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public string Description { get; set; }
    }
}
