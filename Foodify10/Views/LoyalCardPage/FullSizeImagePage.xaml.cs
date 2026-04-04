namespace Foodify10;

public partial class FullSizeImagePage : ContentPage
{
    public FullSizeImagePage(ImageSource imageSource)
    {
        InitializeComponent();
        FullImage.Source = imageSource;
    }
}