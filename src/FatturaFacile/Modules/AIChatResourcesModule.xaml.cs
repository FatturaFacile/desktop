namespace AIDemo {
    public partial class AIChatResourcesModule {
        public AIChatResourcesModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;
            aiChatControl.ChatResources = AIChatResourcesDataProvider.GetData();
        }
    }
}
