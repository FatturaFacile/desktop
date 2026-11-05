using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;
namespace FatturaFacile.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SmartTextEditorPage.xaml
    /// </summary>
    public partial class SmartTextEditorPage : Page
    {
        public SmartTextEditorPage(SmartTextEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
			this.DataContext = new Viewmodel();
        }
    }
    public class Viewmodel : INotifyPropertyChanged
    {
        private string userRole;
        public string UserRole
        {
            get { return userRole; }
            set
            {
                if (userRole != value)
                {
                    userRole = value;
                    OnPropertyChanged(nameof(UserRole));
                }
            }
        }
        public ObservableCollection<Role> Roles { get; }
        private Role selectedRole;
        public Role SelectedRole
        {
            get => selectedRole;
            set
            {
                if (selectedRole != value)
                {
                    selectedRole = value;
                    UserRole = selectedRole.Text;
                    OnPropertyChanged(nameof(SelectedRole));
                }
            }
        }
        public Viewmodel()
        {
            Roles = new ObservableCollection<Role>
            {
                new Role
                {
                    ID = "Role1",
                    Text = "Open-source project maintainer replying to issues",
                    SuggestionPhrases =
                    {
                        "Thanks for reporting this!",
                        "To help us investigate, could you share the steps to reproduce?",
                        "Once we have those details, we can dig deeper.",
                        "Also, can you provide your OS and framework details?",
                        "Thank you for your help in resolving this issue."
                    }
                },
                new Role
                {
                    ID = "Role2",
                    Text = "Employee collaborating with team",
                    SuggestionPhrases =
                    {
                        "Here’s the latest update.",
                        "Let’s discuss this in the next meeting.",
                        "I’ll handle this task.",
                        "Moving this to the next sprint.",
                        "Please review and share your thoughts."
                    }
                },
                new Role
                {
                    ID = "Role3",
                    Text = "Support agent helping customers",
                    SuggestionPhrases =
                    {
                        "Thanks for reaching out!",
                        "I understand this can be frustrating.",
                        "Could you share a screenshot",
                        "We’re checking this and will update soon.",
                        "Try clearing cache and restarting the app."
                    }
                },
                new Role
                {
                    ID = "Role4",
                    Text = "Sales rep responding to client queries",
                    SuggestionPhrases =
                    {
                        "Thank you for your interest!",
                        "I can arrange a quick demo for you.",
                        "I’ll send pricing details shortly.",
                        "Could you share your timeline",
                        "Let me know a good time for a call."
                    }
                }
            };
            SelectedRole = Roles[0];
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class Role
    {
        public string ID { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> SuggestionPhrases { get; } = new List<string>();
    }
}
