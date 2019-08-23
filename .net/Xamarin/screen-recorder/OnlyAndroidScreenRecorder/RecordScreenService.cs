using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Runtime;
using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Environment = Android.OS.Environment;

namespace OnlyAndroidScreenRecorder
{
    [Service(Enabled = true)]
    public class RecordScreenService : Service
    {
        private ServiceHandler _serviceHandler;
        private BroadcastReceiver _screenStateReceiver;
        private MediaRecorder _mediaRecorder;
        
        private MediaProjection _mediaProjection;
        private VirtualDisplay _virtualDisplay;

        private const string ChannelId = "chan01";

        public int ResultCode { get; set; }

        public Intent Data { get; set; }
        
        public override IBinder OnBind(Intent intent) 
        {
            return null; // (no binding needed because of this type of bg service)
        }
        
        //public static Intent CreateIntent(Context context, int resultCode, Intent data) 
        //{
        //    Intent intent = new Intent(context, typeof(RecordScreenService));
        //    intent.PutExtra("resultcode", resultCode);
        //    intent.PutExtra("data", data);
        //    return intent;
        //}

        public override void OnCreate()
        {
            // We will use foreground service type to prevent kill and because screen capture need it

            // We create a notification because it's more UX: user know that service is runing and can close it
            Intent notificationIntent = new Intent(this, typeof(MainActivity));
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);

            var channel = new NotificationChannel(ChannelId, "Kickle", NotificationImportance.Low)
            {
                Description = "Partage d'écran dans l'application Kicle"
            };
            
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);

            var notification = new Notification.Builder(this, ChannelId)
                .SetContentTitle("Kikle Service")
                .SetContentText("En cours de partage")
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true)
                .Build();

            // todo export id
            StartForeground(23, notification);
            
            // We register the B recreiver to manage the case of screen on/off
            // In my Example, i stop the recording when screen is of, and restart new recording when screen is on.
            // I launch a new record when screen rotate too to show you how to get flow with good orientation.
            // This is the more complicated use case but you can choose yours
            // We can use two types of cases :
            // - Stop the screen recording when screen is off
            // - Continue while screen is off
            _screenStateReceiver = new ScreenSharingBroadcastReceiver(this);
            IntentFilter screenStateFilter = new IntentFilter();
            screenStateFilter.AddAction(Intent.ActionScreenOn);
            screenStateFilter.AddAction(Intent.ActionScreenOff);
            screenStateFilter.AddAction(Intent.ActionConfigurationChanged);
            RegisterReceiver(_screenStateReceiver, screenStateFilter);

            HandlerThread thread = new HandlerThread("ServiceStartArguments", (int) ThreadPriority.Background);
            thread.Start();

            _serviceHandler = new ServiceHandler(thread.Looper, this);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Début du partage d'écran...", ToastLength.Short).Show();

            // todo: you can export names if you want
            ResultCode = intent.GetIntExtra("resultcode", 0);
            Data = intent.GetParcelableExtra("data") as Intent;

            if (ResultCode == 0 || Data == null)
            {
                // Checked and throw but it must never append
                throw new IllegalStateException("Data or code is wrong.");
            }

            Message msg = _serviceHandler.ObtainMessage();
            msg.Arg1 = startId;
            _serviceHandler.SendMessage(msg);

            return StartCommandResult.RedeliverIntent;
        }

        
        public override void OnDestroy()
        {
            StopRecording();
            UnregisterReceiver(_screenStateReceiver);
            StopSelf();
            Toast.MakeText(this, "Partage d'écran stoppé", ToastLength.Short).Show();
        }

        public void StartRecording(int resultCode, Intent data) 
        {
            // Recording part
            // Here i use video recording with media recorder to register the flow
            // Maybe this will be a feature in Kicle app so you can reuse the code
            // the flow is registered in Movies directory
            // todo: send the flow with RPC
            MediaProjectionManager projectionManager = (MediaProjectionManager) BaseContext.GetSystemService(MediaProjectionService);
            _mediaRecorder = new MediaRecorder();

            DisplayMetrics metrics = new DisplayMetrics();
            IWindowManager wm = BaseContext.GetSystemService(WindowService).JavaCast<IWindowManager>();
            wm.DefaultDisplay.GetRealMetrics(metrics);

            int mScreenDensity = (int) metrics.DensityDpi;
            int displayWidth = metrics.WidthPixels;
            int displayHeight = metrics.HeightPixels;

            _mediaRecorder.SetVideoSource(VideoSource.Surface);
            _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            _mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            _mediaRecorder.SetVideoEncodingBitRate(8 * 1000 * 1000);
            _mediaRecorder.SetVideoFrameRate(15);
            _mediaRecorder.SetVideoSize(displayWidth, displayHeight);

            string videoDir = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMovies).AbsolutePath;
            
            // Register with uniq id
            long timestamp = DateTime.Now.Ticks;

            // Register with orientation
            string orientation = "portrait";
            if (displayWidth > displayHeight)
            {
                orientation = "landscape";
            }
            
            string filePathAndName = videoDir + "/time_" + timestamp + "_mode_" + orientation + ".mp4";

            _mediaRecorder.SetOutputFile(filePathAndName);

            try 
            {
                _mediaRecorder.Prepare();
            } 
            catch (IllegalStateException) 
            {
                // todo: check if this exception apear (never apeared in dev tests)
                // ignored
            }
            catch (System.Exception)
            {
                // ignored
            }

            _mediaProjection = projectionManager.GetMediaProjection(resultCode, data);
            
            // Flow as been recorded, show it in app too
            Surface surface = _mediaRecorder.Surface;
            _virtualDisplay = _mediaProjection.CreateVirtualDisplay("MainActivity", displayWidth, displayHeight, mScreenDensity, DisplayFlags.Presentation,
                    surface, null, null);
            _mediaRecorder.Start();
        }
        
        public void StopRecording() 
        {
            _mediaRecorder?.Stop();
            _mediaProjection?.Stop();
            _mediaRecorder?.Release();
            _virtualDisplay?.Release();
        }
    }

    public class ServiceHandler : Handler
    {
        private readonly RecordScreenService _service;

        public ServiceHandler(Looper looper, RecordScreenService service) : base(looper)
        {
            _service = service;
        }

        public override void HandleMessage(Message msg)
        {
            if (_service.ResultCode != 0) 
            {
                _service.StartRecording(_service.ResultCode, _service.Data);
            }
        }
    }

    public class ScreenSharingBroadcastReceiver : BroadcastReceiver
    {
        private readonly RecordScreenService _service;

        public ScreenSharingBroadcastReceiver(RecordScreenService service)
        {
            _service = service;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            switch (action)
            {
                case Intent.ActionScreenOn:
                    _service.StartRecording(_service.ResultCode, _service.Data);
                    break;
                case Intent.ActionScreenOff:
                    _service.StopRecording();
                    break;
                case Intent.ActionConfigurationChanged:
                    _service.StopRecording();
                    _service.StartRecording(_service.ResultCode, _service.Data);
                    break;
            }
        }
    }
}