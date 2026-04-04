using CommunityToolkit.Maui.Media;
using Foodify10.Services.Interfaces;
using System.Globalization;

namespace Foodify10.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        public async Task<string?> ListenOnceAsync(string cultureName, CancellationToken cancellationToken = default)
        {
            var isGranted = await SpeechToText.Default.RequestPermissions(cancellationToken);
            if (!isGranted)
                throw new InvalidOperationException("Нет доступа к микрофону");

            var tcs = new TaskCompletionSource<string?>();

            void Handler(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
            {
                SpeechToText.Default.RecognitionResultCompleted -= Handler;

                if (e.RecognitionResult.IsSuccessful && !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
                    tcs.TrySetResult(e.RecognitionResult.Text);
                else
                    tcs.TrySetResult(null);
            }

            SpeechToText.Default.RecognitionResultCompleted -= Handler;
            SpeechToText.Default.RecognitionResultCompleted += Handler;

            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.GetCultureInfo(cultureName)
            };

            await SpeechToText.Default.StartListenAsync(options, cancellationToken);

            return await tcs.Task;
        }
    }
}