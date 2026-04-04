using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

namespace Foodify10.Platforms.Android
{
    [BroadcastReceiver(Label = "Foodify | Список продуктов", Exported = true)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/widget_info")]
    public class ShoppingWidgetProvider : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var widgetId in appWidgetIds)
            {
                // Создаем Layout виджета (нужно создать XML-файл в Platforms/Android/Resources/layout/widget_layout.xml)
                var views = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);

                views.SetTextViewText(Resource.Id.widgetTitle, "🛒 Списки покупок");

                var intent = new Intent(context, typeof(MainActivity));
                var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);
                views.SetOnClickPendingIntent(Resource.Id.widgetRoot, pendingIntent);

                appWidgetManager.UpdateAppWidget(widgetId, views);
            }
        }
    }
}
