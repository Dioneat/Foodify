using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class VibrationService : IVibrationService
    {
        public void Vibrate(int milliseconds)
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
            }
            catch (FeatureNotSupportedException)
            {
            }
            catch (Exception)
            {
            }
        }
    }
}