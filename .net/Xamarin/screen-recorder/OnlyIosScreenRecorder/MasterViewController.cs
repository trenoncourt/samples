using System;
using System.Collections.Generic;

using UIKit;
using Foundation;
using AVFoundation;
using ReplayKit;

namespace OnlyIosScreenRecorder
{
	public partial class MasterViewController : UITableViewController
	{
		DataSource _dataSource;
		RPScreenRecorder _screenRecorder = RPScreenRecorder.SharedRecorder;
		AVAssetWriter _assetWriter;
		AVAssetWriterInput _videoInput;
		RPBroadcastController _broadcastController = new RPBroadcastController();

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

			var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, AddNewItem);
			addButton.AccessibilityLabel = "addButton";
			NavigationItem.RightBarButtonItem = addButton;

			TableView.Source = _dataSource = new DataSource(this);
		}

		private NSUrl GetFilePath(string fileName)
		{
			string documentPath = new NSFileManager().GetUrl(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, null, true, out var error).ToString();
			string docs = $"{documentPath}{fileName}";
			
			return error == null ? new NSUrl(docs) : null;
		}

        public void StartScreenSharing()
        {
	        _assetWriter = new AVAssetWriter(GetFilePath($"kickle_{DateTime.Now.Ticks}.mp4"), AVFileType.AppleM4A, out _);

            _videoInput = new AVAssetWriterInput(AVMediaType.Video, new AVVideoSettingsCompressed
            {
	            Codec = AVVideoCodec.H264,
	            Width = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Width),
	            Height = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Height)
            }) {ExpectsMediaDataInRealTime = true};


            _assetWriter.AddInput(_videoInput);
            
            RPBroadcastActivityViewController.LoadBroadcastActivityViewController((broadcast, error) => { broadcast.Delegate = this; });

            if (_screenRecorder.Available)
            {
	            _screenRecorder.StartCaptureAsync((buffer, sampleBufferType, error) =>
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
	        if (_screenRecorder.Recording)
	        {
		        StopScreenSharing();
		        return;
	        }
	        
		    StartScreenSharing();
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
