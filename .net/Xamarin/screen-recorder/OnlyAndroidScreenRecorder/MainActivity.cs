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
        private MediaProjection mediaProjection;
        private MediaProjectionManager mediaProjectionManager;
        private int resultCode;
        private Intent resultData;
        private VirtualDisplay virtualDisplay;
        private SurfaceView surfaceView;
        private int screenDensity;
        private Surface surface;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            if (savedInstanceState != null)
            {
                resultCode = savedInstanceState.GetInt("result_code");
                resultData = (Intent)savedInstanceState.GetParcelable("result_data");
            }

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            surfaceView = FindViewById<SurfaceView>(Resource.Id.surface);
            surface = surfaceView.Holder.Surface;

            var metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            screenDensity = (int)metrics.DensityDpi;

            mediaProjectionManager = (MediaProjectionManager)GetSystemService(MediaProjectionService);

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

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            if (virtualDisplay == null)
                StartScreenCapture();
            else
                StopScreenCapture();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 1)
            {
                if (resultCode != Result.Ok)
                {
                    Toast.MakeText(this, "cancel", ToastLength.Short).Show();
                    return;
                }
                this.resultCode = (int)resultCode;
                resultData = data;
                SetUpMediaProjection();
                SetUpVirtualDisplay();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (resultData != null)
            {
                outState.PutInt("result_code", resultCode);
                outState.PutParcelable("result_data", resultData);
            }
        }

        private void SetUpMediaProjection()
        {
            mediaProjection = mediaProjectionManager.GetMediaProjection(resultCode, resultData);
        }

        private void SetUpVirtualDisplay()
        {
            virtualDisplay = mediaProjection.CreateVirtualDisplay("ScreenCapture",
                surfaceView.Width, surfaceView.Height, screenDensity,
                (DisplayFlags)VirtualDisplayFlags.AutoMirror, surface, null, null);
        }

        private void StartScreenCapture()
        {
            if (surface == null)
                return;
            if (mediaProjection != null)
            {
                SetUpVirtualDisplay();
            }
            else if (resultCode != 0 && resultData != null)
            {
                SetUpMediaProjection();
                SetUpVirtualDisplay();
            }
            else
            {
                // This initiates a prompt for the user to confirm screen projection.
                StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), 1);
            }
        }

        private void StopScreenCapture()
        {
            if (virtualDisplay == null)
                return;

            virtualDisplay.Release();
            virtualDisplay = null;
        }
    }
}

