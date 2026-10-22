using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public class AuthoredStyleRequest : TextRequest {
        public AuthoredStyleRequest(string author, string text) : base(text) {
            Author = author;
        }
        public string Author { get; set; }
    }
}
