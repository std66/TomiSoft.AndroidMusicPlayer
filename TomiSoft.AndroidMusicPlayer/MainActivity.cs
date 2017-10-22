using System;
using Android.App;
using Android.Widget;
using Android.OS;
using TomiSoft.MP3Player.Playback.BASS;
using TomiSoft.MP3Player.Playback;
using TomiSoft.MP3Player.MediaInformation;
using System.Threading.Tasks;
using TomiSoft.MP3Player.Common.Playback;
using System.ComponentModel;

namespace TomiSoft.AndroidMusicPlayer {
	[Activity(Label = "TomiSoft MP3 Player", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity {
		private readonly string Filename = "/storage/sdcard1/Music/Aoki Lapis - Never fading.mp3";

		private IPlaybackManager PlaybackManager;
		private IAudioPeakMeter PeakMeter;

		private ProgressBar LeftPeakMeter;
		private ProgressBar RightPeakMeter;
		private SeekBar PositionTrackbar;
		private TextView SongTitle;

		private bool UpdatePositionTrackbar = true;
		
		private void InitializeComponent() {
			Button button = FindViewById<Button>(Resource.Id.MyButton);
			button.Click += btnPlay_Click;

			this.LeftPeakMeter = FindViewById<ProgressBar>(Resource.Id.peak_left);
			this.RightPeakMeter = FindViewById<ProgressBar>(Resource.Id.peak_right);
			this.LeftPeakMeter.Max = 32767;
			this.RightPeakMeter.Max = 32767;

			this.PositionTrackbar = FindViewById<SeekBar>(Resource.Id.PositionTrackbar);
			this.PositionTrackbar.StopTrackingTouch += OnSetPlaybackPosition;
			this.PositionTrackbar.StartTrackingTouch += (o, e) => this.UpdatePositionTrackbar = false;

			this.SongTitle = FindViewById<TextView>(Resource.Id.SongTitle);

			FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(Resource.Drawable.ApplicationIcon);
		}

		private void OnSetPlaybackPosition(object sender, SeekBar.StopTrackingTouchEventArgs e) {
			if (this.PlaybackManager == null)
				return;

			this.PlaybackManager.Position = this.PositionTrackbar.Progress;
			this.UpdatePositionTrackbar = true;
		}

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			InitializeComponent();
			
			BassManager.Load($"{this.ApplicationInfo.NativeLibraryDir}/");
			BassManager.InitializeOutputDevice();
			this.PlaybackManager = PlaybackFactory.NullPlayback(100);
		}

		private void OnUpdate(object sender, PropertyChangedEventArgs e) {
			if (this.PlaybackManager == null)
				return;
			
			if (this.PeakMeter != null) {
				this.LeftPeakMeter.Progress = PeakMeter.LeftPeak;
				this.RightPeakMeter.Progress = PeakMeter.RightPeak;
			}

			if (this.UpdatePositionTrackbar) {
				this.PositionTrackbar.Max = (int)this.PlaybackManager.Length;
				this.PositionTrackbar.Progress = (int)this.PlaybackManager.Position;
			}
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			await this.AttachPlayer(Filename);
		}

		private async Task AttachPlayer(string Source) {
			if (this.PlaybackManager != null) {
				this.PlaybackManager.PropertyChanged -= this.OnUpdate;
				this.PlaybackManager.Dispose();
			}

			this.PlaybackManager = await PlaybackFactory.LoadMedia(new SongInfo(Filename, 0, true));
			this.PeakMeter = this.PlaybackManager as IAudioPeakMeter;

			this.PlaybackManager.PropertyChanged += this.OnUpdate;
			
			this.SongTitle.Text = this.PlaybackManager.SongInfo.Title;

			PlaybackManager.Volume = 100;
			PlaybackManager.Play();
		}
	}
}

