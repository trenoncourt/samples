using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace ScreenRecorder.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() =>
            {
                var mediaProjectionManager = (MediaProjectionManager)Activity.GetSystemService(Context.MediaProjectionService)
            }
            );
        }

        public ICommand OpenWebCommand { get; }
    }
}