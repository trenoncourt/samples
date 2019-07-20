using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    [Service(IsolatedProcess = true)]
    public class RecordScreenService : Service
    {
        private ServiceHandler _serviceHandler;
        private BroadcastReceiver _screenStateReceiver;
        private MediaRecorder _mediaRecorder;
        
        private MediaProjection _mediaProjection;
        private VirtualDisplay _virtualDisplay;

        public int ResultCode { get; set; }

        public Intent Data { get; set; }
        
        public override IBinder OnBind(Intent intent) 
        {
            return null; // (no binding)
        }
        
        public static Intent CreateIntent(Context context, int resultCode, Intent data) 
        {
            Intent intent = new Intent(context, typeof(RecordScreenService));
            intent.PutExtra("resultcode", resultCode);
            intent.PutExtra("data", data);
            return intent;
        }

        public override void OnCreate()
        {
            // run this service as foreground service to prevent it from getting killed
            // when the main app is being closed
            Intent notificationIntent = new Intent(this, this.GetType());
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);

            Notification notification = new Notification.Builder(this)
                    .SetContentTitle("DataRecorder")
                    .SetContentText("Your screen is being recorded and saved to your phone.")
                    .SetSmallIcon(Resource.Mipmap.ic_launcher)
                    .SetContentIntent(pendingIntent)
                    .SetTicker("Tickertext")
                    .Build();
            
            StartForeground(23, notification);
            
            // register receiver to check if the phone screen is on or off
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

        public override void OnDestroy()
        {
            StopRecording();
            UnregisterReceiver(_screenStateReceiver);
            StopSelf();
            Toast.MakeText(this, "Screen sharing has been stoped", ToastLength.Short).Show();
        }

        public void StartRecording(int resultCode, Intent data) 
        {
            MediaProjectionManager projectionManager = (MediaProjectionManager) BaseContext.GetSystemService(MediaProjectionService);
            _mediaRecorder = new MediaRecorder();

            DisplayMetrics metrics = new DisplayMetrics();
            IWindowManager wm = (IWindowManager) BaseContext.GetSystemService(WindowService);
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
            long timestamp = DateTime.Now.Ticks;

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
            catch (IllegalStateException e) 
            {
            }
            catch (System.Exception e)
            {
                // ignored
            }

            _mediaProjection = projectionManager.GetMediaProjection(resultCode, data);
            Surface surface = _mediaRecorder.Surface;
            _virtualDisplay = _mediaProjection.CreateVirtualDisplay("MainActivity", displayWidth, displayHeight, mScreenDensity, DisplayFlags.Presentation,
                    surface, null, null);
            _mediaRecorder.Start();
        }
        
        public void StopRecording() 
        {
            _mediaRecorder.Stop();
            _mediaProjection.Stop();
            _mediaRecorder.Release();
            _virtualDisplay.Release();
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
            if (_service.ResultCode == 1) 
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