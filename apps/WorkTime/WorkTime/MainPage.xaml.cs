namespace WorkTime
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
        }


        private void InClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(InBtn.Text);
        }

        private void OutBtn_Clicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(OutBtn.Text);
        }
    }
}