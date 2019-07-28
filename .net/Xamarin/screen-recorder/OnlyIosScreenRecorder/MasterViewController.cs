using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using ReplayKit;

namespace OnlyIosScreenRecorder
{
	public partial class MasterViewController : UITableViewController, IRPBroadcastActivityViewControllerDelegate, IRPBroadcastControllerDelegate, IRPScreenRecorderDelegate
    {
		DataSource _dataSource;
		RPScreenRecorder _screenRecorder = RPScreenRecorder.SharedRecorder;
		AVAssetWriter _assetWriter;
		AVAssetWriterInput _videoInput;
		RPBroadcastController _broadcastController = new RPBroadcastController();
        UIBarButtonItem _addButton;
        
        // utilisation du type UIView car RPSystemBroadcastPickerView existe seulement à partir de iOS 12
        private UIView _broadcastPickerView;
        private readonly UIActivityIndicatorView _spinerIndicator = new UIActivityIndicatorView();
        


        protected MasterViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = NSBundle.MainBundle.GetLocalizedString("Master", "Master");
			SplitViewController.PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;

			// Perform any additional setup after loading the view, typically from a nib.
			NavigationItem.LeftBarButtonItem = EditButtonItem;
            
            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, AddNewItem);
            _addButton.AccessibilityLabel = "Start recording";
			NavigationItem.RightBarButtonItem = _addButton;

			TableView.Source = _dataSource = new DataSource(this);

            // The setter fires an availability changed event, but we check rather than rely on this implementation detail.
            RPScreenRecorder.SharedRecorder.Delegate = this;
            CheckRecordingAvailability();

            NSNotificationCenter.DefaultCenter.AddObserver(UIScreen.CapturedDidChangeNotification, UIScreen.MainScreen, NSOperationQueue.MainQueue, notification =>
            {
	            if (_broadcastPickerView != null)
	            {
		            _addButton.Enabled = !UIScreen.MainScreen.Captured;
		            if (UIScreen.MainScreen.Captured)
			            _spinerIndicator?.StartAnimating();
		            else
			            _spinerIndicator?.StopAnimating();
	            }
            });

            // check de la version d'ios (feature présente à partir de la version 12)
            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
            {
	            SetupPickerView();
            }
		}

		private void SetupPickerView()
		{
			var pickerView = new RPSystemBroadcastPickerView(frame: new CGRect(0, 0, View.Bounds.Width, 80))
			{
				TranslatesAutoresizingMaskIntoConstraints = false, 
				PreferredExtension = "com.kickle.app.BroadcastExtension" // todo
			};
			
			UIButton button = pickerView.Subviews.FirstOrDefault() as UIButton;
			if (button?.ImageView != null)
			{
				button.ImageView.TintColor = UIColor.White;
			}

			View.AddSubview(pickerView);
			_broadcastPickerView = pickerView;
			_addButton.Enabled = false;
			
			var centerX = NSLayoutConstraint.Create(pickerView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _addButton, NSLayoutAttribute.CenterX,
                                         multiplier: 1,
                                         constant: 0);
			View.AddConstraint(centerX);
	        var centerY = NSLayoutConstraint.Create(pickerView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _addButton, NSLayoutAttribute.CenterY,
	                                         multiplier: 1,
	                                         constant: -10);
	        View.AddConstraint(centerY);
	        var width = NSLayoutConstraint.Create(pickerView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _addButton, NSLayoutAttribute.Width,
	                                       multiplier: 1,
	                                       constant: 0);
	        View.AddConstraint(width);
	        var height = NSLayoutConstraint.Create(pickerView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, _addButton, NSLayoutAttribute.Height,
	                                        multiplier: 1,
	                                        constant: 0);
	        View.AddConstraint(height);
		}
		
		private void StartBroadcast(object sender)
		{
			_broadcastController?.FinishBroadcast(error =>
			{
				_spinerIndicator.StopAnimating();
				_broadcastController = null;
				_addButton.Enabled = true;
			});

			RPBroadcastActivityViewController.LoadBroadcastActivityViewController("com.kickle.app.BroadcastExtensionUI" /* todo */, (controller, error) =>
			{
				if (controller == null) return;
				controller.Delegate = this;
				controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
				PresentViewController(controller, true, () => { });
			});
		}

		private NSUrl GetFilePath(string fileName)
		{
			string documentPath = new NSFileManager().GetUrl(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, null, true, out var error).ToString();
			string docs = $"{documentPath}{fileName}";
			
			return error == null ? new NSUrl(docs) : null;
		}

        public async Task StartScreenSharing()
        {
	        _addButton.Enabled = false;
	        if (_broadcastPickerView != null)
	        {
		        _broadcastPickerView.Hidden = true;
	        }

	        _screenRecorder.MicrophoneEnabled = false;
	        _screenRecorder.CameraEnabled = false;
	        
	        _assetWriter = new AVAssetWriter(GetFilePath($"kickle_{DateTime.Now.Ticks}.mp4"), AVFileType.AppleM4A, out _);

            _videoInput = new AVAssetWriterInput(AVMediaType.Video, new AVVideoSettingsCompressed
            {
	            Codec = AVVideoCodec.H264,
	            Width = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Width),
	            Height = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Height)
            }) {ExpectsMediaDataInRealTime = true};


            _assetWriter.AddInput(_videoInput);
            
//            RPBroadcastActivityViewController.LoadBroadcastActivityViewController((broadcast, error) => { broadcast.Delegate = this; });

	        if (_screenRecorder.Available)
	        {
		        await _screenRecorder.StartCaptureAsync((buffer, sampleBufferType, error) =>
		        {
			        if (buffer.DataIsReady)
			        {
				        if (_assetWriter.Status == AVAssetWriterStatus.Failed)
				        {
					        return;
				        }

				        if (_assetWriter.Status == AVAssetWriterStatus.Unknown)
				        {
					        _assetWriter.StartWriting();
					        _assetWriter.StartSessionAtSourceTime(buffer.PresentationTimeStamp);
				        }

				        if (sampleBufferType == RPSampleBufferType.Video)
				        {
					        if (_videoInput.ReadyForMoreMediaData)
					        {
						        _videoInput.AppendSampleBuffer(buffer);
					        }
				        }
			        }
		        });
	        }
        }
        
        public void StopScreenSharing()
        {
	        _screenRecorder.StopCapture(error =>
	        {
		        if (error == null)
		        {
			        _assetWriter.FinishWriting(() => { });
		        }
	        });
        }

        void AddNewItem(object sender, EventArgs args)
        {
	        _addButton.Enabled = false;
	        if (_screenRecorder.Recording)
	        {
		        StopScreenSharing();
		        return;
	        }
	        
		    StartScreenSharing();
        }
        
        private void StartBroadcast()
        {
	        _broadcastController?.StartBroadcast(error =>
	        {
		        DispatchQueue.MainQueue.DispatchSync(() =>
		        {
			        if (error != null)
			        {
				        _spinerIndicator.StartAnimating();
			        }
		        });
	        });
        }

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showDetail")
			{
				var controller = (DetailViewController)((UINavigationController)segue.DestinationViewController).TopViewController;
				var indexPath = TableView.IndexPathForSelectedRow;
				var item = _dataSource.Objects[indexPath.Row];

				controller.SetDetailItem(item);
				controller.NavigationItem.LeftBarButtonItem = SplitViewController.DisplayModeButtonItem;
				controller.NavigationItem.LeftItemsSupplementBackButton = true;
			}
		}

        public void DidFinish(RPBroadcastActivityViewController broadcastActivityViewController, RPBroadcastController broadcastController, NSError error)
        {
	        DispatchQueue.MainQueue.DispatchSync(() =>
	        {
		        _broadcastController = broadcastController;
		        if (_broadcastController != null)
		        {
			        _broadcastController.Delegate = this;
		        }


		        broadcastActivityViewController.DismissViewController(true, () => { this.StartBroadcast(); });
	        });
        }

        private void CheckRecordingAvailability()
        {
            bool isScreenRecordingAvailable = RPScreenRecorder.SharedRecorder.Available;
            _addButton.Enabled = isScreenRecordingAvailable;
        }

        class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString("Cell");
			readonly List<object> objects = new List<object>();
			readonly MasterViewController controller;

			public DataSource(MasterViewController controller)
			{
				this.controller = controller;
			}

			public IList<object> Objects
			{
				get { return objects; }
			}

			// Customize the number of sections in the table view.
			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return objects.Count;
			}

			// Customize the appearance of table view cells.
			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);

				cell.TextLabel.Text = objects[indexPath.Row].ToString();

				return cell;
			}

			public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
			{
				// Return false if you do not want the specified item to be editable.
				return true;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete)
				{
					// Delete the row from the data source.
					objects.RemoveAt(indexPath.Row);
					controller.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
				}
				else if (editingStyle == UITableViewCellEditingStyle.Insert)
				{
					// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
				}
			}
		}
	}
}
