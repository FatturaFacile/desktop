using System;
using System.Windows;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Xpf.Core;

namespace AIDemo {
    public class AIExceptionHandler : IAIExceptionHandler {
        public void ProcessException(AIExceptionArgs args) {
            if(!(args.Exception is AIDemoException))
                args.Exception = new Exception("Something went wrong.", args.Exception);

            if(args.AIExtension is SmartSearchAIExtension || args.AIExtension is SmartAutoCompleteExtension) {
                ThemedMessageBox.Show("AI Service Error", args.Exception.Message, MessageBoxButton.OK);
            }
        }
    }

    public class AIDemoException : Exception {
        public AIDemoException(string message) : base(message) { }
    }
}
