namespace AIDemo {
    public partial class SmartAutoCompleteModule {
        public SmartAutoCompleteModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;
        }
    }

    public class UserRoleItem {
        public string UserRole { get; set; }
        public string DisplayText { get; set; }
    }
}
