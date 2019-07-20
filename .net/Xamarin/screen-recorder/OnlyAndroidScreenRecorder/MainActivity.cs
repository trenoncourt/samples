using System;
using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace OnlyAndroidScreenRecorder
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MediaProjectionManager _projectionManager;
        private bool _isSharing = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            
            _projectionManager = (MediaProjectionManager) GetSystemService(MediaProjectionService);

//            if (savedInstanceState != null)
//            {
//                resultCode = savedInstanceState.GetInt("result_code");
//                resultData = (Intent)savedInstanceState.GetParcelable("result_data");
//            }
//
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
//
//            surfaceView = FindViewById<SurfaceView>(Resource.Id.surface);
//            surface = surfaceView.Holder.Surface;
//
//            var metrics = new DisplayMetrics();
//            WindowManager.DefaultDisplay.GetMetrics(metrics);
//            screenDensity = (int) metrics.DensityDpi;

            _isSharing = IsServiceRunning(typeof(RecordScreenService));

            fab.Click += FabOnClick;
        }
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        
        private bool IsServiceRunning(Type serviceType) 
        {
            ActivityManager manager = (ActivityManager) GetSystemService(Context.ActivityService);
            
            // todo
            foreach (ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue)) 
            {
                if (service.Class.Name == serviceType.Name) 
                {
                    return true;
                }
            }

            return false;
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            if (!_isSharing)
                StartActivityForResult(_projectionManager.CreateScreenCaptureIntent(), 1);
            else
                StopScreenCapture();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode != 1) return;
            
            if (resultCode == Result.Ok) 
            {
                StartScreenCapture((int) resultCode, data);
            }
            else
            {  
                Toast.MakeText(this, "Permission Denied..", ToastLength.Short).Show();
                _isSharing = false;
                return;
            }
        }

        private void StartScreenCapture(int resultCode, Intent data)
        {
            Intent intent = RecordScreenService.CreateIntent(this, resultCode, data);
            StartService(intent);
        }

        private void StopScreenCapture()
        {
            Intent intent = new Intent(this, typeof(RecordScreenService));
            StopService(intent);
        }
    }
}

