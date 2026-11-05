using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FatturaFacile.Wpf.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Chat;
namespace FatturaFacile.Wpf.Views
{
    /// <summary>
    /// Interaction logic for AIAssistViewPage.xaml
    /// </summary>
    public partial class AIAssistViewPage : Page
    {
        public AIAssistViewPage(AIAssistViewViewModel viewModel)
        {           
            InitializeComponent();
            DataContext = viewModel;
			this.DataContext = new ViewModel();
        }
    }
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<object> chats;
        private Author currentUser;
        private TypingIndicator typingIndicator;
        public ViewModel()
        {
            this.Chats = new ObservableCollection<object>();
            this.CurrentUser = new Author { Name = "John" };
            TypingIndicator = new TypingIndicator { Author = new Author { Name = "AI" } };
            this.GenerateMessages();
        }
        private async void GenerateMessages()
        {
            this.Chats.Add(new TextMessage { Author = CurrentUser, Text = "What is WPF?" });
            await Task.Delay(1000);
            this.Chats.Add(new TextMessage { Author = new Author { Name = "Bot" }, Text = "WPF is a user interface layer that contains modern controls and styles for building Windows apps." });
        }
        public ObservableCollection<object> Chats
        {
            get
            {
                return this.chats;
            }
            set
            {
                this.chats = value;
                RaisePropertyChanged("Messages");
            }
        }
        public Author CurrentUser
        {
            get
            {
                return this.currentUser;
            }
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }
        public TypingIndicator TypingIndicator
        {
            get
            {
                return this.typingIndicator;
            }
            set
            {
                this.typingIndicator = value;
                RaisePropertyChanged("TypingIndicator");
            }
        }
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
