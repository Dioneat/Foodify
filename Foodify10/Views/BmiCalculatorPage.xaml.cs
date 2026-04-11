using Foodify10.ViewModels;

namespace Foodify10.Views
{
    public partial class BmiCalculatorPage : ContentPage
    {
        public BmiCalculatorPage(BmiCalculatorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}