using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Wpf;
using DevExpress.AIIntegration.Wpf.Internal;
using DevExpress.Xpf.Editors;

namespace AIDemo.Modules.CodeExamples {
    class AuthoredStyleBehavior : TextModificationBehavior {
        static AuthoredStyleBehavior() {
            RegisterBehaviorSource<TextEdit, AuthoredStyleBehavior>(te => new TextEditBehaviorSource(te));
            AIExtensionsContainerDesktop.Default.Register<AuthoredStyleRequest, AuthoredStyleExtension>();
        }

        public AuthoredStyleBehavior() {
            AuthoredStyleCommand = CreateCommand<string>(AuthoredStyleAsync);
        }

        ICommand AuthoredStyleCommand { get; set; }

        Task AuthoredStyleAsync(string author) => ExecuteAsync(
            input => new AuthoredStyleRequest(author, input),
            (container, request, token) => {
                var extension = (AuthoredStyleExtension)container.GetExtension(typeof(AuthoredStyleRequest));
                var result = extension.ExecuteAsync(request, token);
                return result;
        });

        protected override ItemInfo BuildItem() {
            return new ItemInfo {
                Caption = "Authored Style",
                Items = new[] { "Mark Twain", "Ernest Hemingway", "Maya Angelou" }.Select(author => new ItemInfo {
                    Caption = author,
                    Command = AuthoredStyleCommand,
                    CommandParameter = author
                }).ToList()
            };
        }
    }
}
