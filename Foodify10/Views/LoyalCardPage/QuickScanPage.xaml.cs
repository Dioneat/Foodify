namespace Foodify10;

public partial class QuickScanPage : ContentPage
{
    public event Action<string> OnCodeScanned;
    private bool _isAnalysisComplete = false;
    public QuickScanPage()
    {
        InitializeComponent();
        BarcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.Code128
                | ZXing.Net.Maui.BarcodeFormat.Ean13
                | ZXing.Net.Maui.BarcodeFormat.Ean8
                | ZXing.Net.Maui.BarcodeFormat.Code39
                | ZXing.Net.Maui.BarcodeFormat.UpcA
                | ZXing.Net.Maui.BarcodeFormat.QrCode
                | ZXing.Net.Maui.BarcodeFormat.Itf,
            TryHarder = true,
            AutoRotate = true,
            TryInverted = true
        };
    }

    private void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        if (_isAnalysisComplete) return;
        var first = e.Results.FirstOrDefault();
        if (first != null)
        {
            _isAnalysisComplete = true;
            
            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
            catch (FeatureNotSupportedException) {  }

            // 2. Возвращаем результат в главный поток
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                OnCodeScanned?.Invoke(first.Value);
                await Navigation.PopModalAsync(); 
            });
        }
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_isAnalysisComplete) return;
        _isAnalysisComplete = true;
        await Navigation.PopModalAsync();
    }
}