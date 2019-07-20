using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OnlyAndroidScreenRecorder
{
    public class Constants
    {
        public const int REGISTER_MESSENGER = 1;
        public const int UNREGISTER_MESSENGER = 2;
        public const int STOP_RECORD = 4;
        public const int SEND_TO_MESSENGER = 5;
        public const int START_RECORD = 7;
        public const int MSG_REGISTER_ACK_READY = 9;
        public const int MSG_REGISTER_ACK_NOT_READY = 10;
        public const string EXTRA_SURFACE = "EXTRA_SURFACE";
        public const string SCREEN_WIDTH = "SCREEN_WIDTH";
        public const string SCREEN_HEIGHT = "SCREEN_HEIGHT";
    }

    [Service(IsolatedProcess = true)]
    public class RecordScreenService : Service
    {
        private Messenger _messenger;
        List<Messenger> messengerList = new List<Messenger>();
        CurrentState currentState = CurrentState.Ready;
        MediaRecorder MediaRecorder;
        IWindowManager wm;

        public override IBinder OnBind(Intent intent)
        {
            return _messenger.Binder;
        }



        public override void OnCreate()
        {
            base.OnCreate();
            //WindowManagerLayoutParams layoutParams = new WindowManager
            //IWindowManager.LayoutParams @params = new WindowManager.LayoutParams(
            //    WindowManager.LayoutParams.TYPE_TOAST, WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
            //    | WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL |
            //    WindowManager.LayoutParams.FLAG_WATCH_OUTSIDE_TOUCH,
            //    PixelFormat.TRANSLUCENT);
            //@params.gravity = Gravity.RIGHT | Gravity.TOP;
            //@params.setTitle("Load Average");
            //    wm = (WindowManager)getSystemService(WINDOW_SERVICE);
            //@params.width = WindowManager.LayoutParams.WRAP_CONTENT;
            //@params.height = WindowManager.LayoutParams.WRAP_CONTENT;

            //mView = new Button(this);
            //setViewState(VIEW_STATE_READY);
            //mView.setOnTouchListener(new ViewTouchListener(getBaseContext(), wm, params));
            //mView.setOnClickListener(this);
            //wm.addView(mView, params);
            //uiHandler = new Handler();

            //videoPath = initRecorder();
            InitRecorder();
            _messenger = new Messenger(new IncomingHandler(messengerList, MediaRecorder));
        }

        private string InitRecorder()
        {
            MediaRecorder = new MediaRecorder();
            MediaRecorder.SetAudioSource(AudioSource.Mic);
            MediaRecorder.SetVideoSource(VideoSource.Surface);
            MediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);

            string videoPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + string.Format("/{0}_video.3gpp", DateTime.Now.ToString("yyMMddHHmmssZ"));

            MediaRecorder.SetOutputFile(file.getAbsolutePath());
            MediaRecorder.SetVideoSize(DISPLAY_WIDTH, DISPLAY_HEIGHT);
            MediaRecorder.SetVideoEncoder(MediaRecorder.VideoEncoder.H264);
            MediaRecorder.SetAudioEncoder(MediaRecorder.AudioEncoder.AMR_NB);
            MediaRecorder.SetVideoFrameRate(100);
            MediaRecorder.SetVideoEncodingBitRate(4096 * 1000);

            int rotation = wm.getDefaultDisplay().getRotation();
            int orientation = 0;
            MediaRecorder.setOrientationHint(rotation);
            MediaRecorder.prepare();
        }

        public void setViewState(CurrentState state)
        {

            if (state == currentState)
            {
                return;
            }
            currentState = state;

            switch (state)
            {
                case CurrentState.Ready:
                    mView.setVisibility(View.VISIBLE);
                    mView.setText(R.string.start_recording);
                    break;
                case CurrentState.Start:
                    mView.setVisibility(View.VISIBLE);
                    mView.setText(R.string.stop_recording);
                    break;
                default:
                    mView.setVisibility(View.GONE);
            }


        }
    }
    public class IncomingHandler : Handler
    {
        List<Messenger> _messengerList = new List<Messenger>();
        CurrentState currentState = CurrentState.Ready;
        MediaRecorder MediaRecorder;

        public IncomingHandler(List<Messenger> messengerList, MediaRecorder mediaRecorder)
        {
            _messengerList = messengerList;
            MediaRecorder = mediaRecorder;
        }

        public override void HandleMessage(Message msg)
        {
            switch (msg.What)
            {
                case Constants.REGISTER_MESSENGER:
                    _messengerList.Add(msg.ReplyTo);

                    if (currentState == CurrentState.Ready /* && videoPath != null */)
                    {
                        Message.Obtain(null, Constants.MSG_REGISTER_ACK_READY, MediaRecorder.Surface);
                    }
                    else
                    {
                        Message.Obtain(null, Constants.MSG_REGISTER_ACK_NOT_READY, null);
                    }
                    break;
                case Constants.UNREGISTER_MESSENGER:
                    _messengerList.Remove(msg.ReplyTo);
                    break;
                case Constants.START_RECORD:
                    // set view state to start
                    MediaRecorder.Start();
                    break;
                case Constants.SEND_TO_MESSENGER:

                    foreach (var messenger in _messengerList)
                    {
                        try
                        {
                            messenger.Send(Message.Obtain(null, msg.Arg1, msg.Obj));
                        }
                        catch (RemoteException e)
                        {
                            // todo
                        }
                    }
                    break;
            }
        }
    }


    public enum CurrentState
    {
        Ready = 1,
        Start = 2
    }
}