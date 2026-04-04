using Foodify10.ViewModels;

namespace Foodify10;

public partial class BmrPage : ContentPage
{
    private readonly BmrPageViewModel _viewModel;

    public BmrPage(BmrPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnLabelTapped(object sender, TappedEventArgs e)
    {
        string param = e.Parameter?.ToString() ?? string.Empty;

        if (param == "Height")
            ToggleEdit(LblHeight, EditHeight, _viewModel.Height);
        else if (param == "Weight")
            ToggleEdit(LblWeight, EditWeight, _viewModel.Weight);
        else if (param == "Age")
            ToggleEdit(LblAge, EditAge, _viewModel.Age);
    }

    private void ToggleEdit(Label lbl, Entry entry, double value)
    {
        lbl.IsVisible = false;
        entry.IsVisible = true;
        entry.Text = ((int)value).ToString();
        entry.Focus();
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        ((Entry)sender).Unfocus();
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;

        if (entry == EditHeight)
            _viewModel.SetHeightFromText(entry.Text);
        else if (entry == EditWeight)
            _viewModel.SetWeightFromText(entry.Text);
        else if (entry == EditAge)
            _viewModel.SetAgeFromText(entry.Text);

        entry.IsVisible = false;
        LblHeight.IsVisible = true;
        LblWeight.IsVisible = true;
        LblAge.IsVisible = true;
    }
}