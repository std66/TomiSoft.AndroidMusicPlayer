using System.IO;
using TomiSoft.MP3Player.MediaInformation;
using TomiSoft.MP3Player.Utils.Extensions;
using Un4seen.Bass;

namespace TomiSoft.MP3Player.Playback.BASS {
	/// <summary>
	/// Provides a file playback method using BASS.
	/// </summary>
	internal class LocalAudioFilePlayback : BassPlaybackAbstract {
		/// <summary>
		/// Stores the loaded file's path.
		/// </summary>
		private readonly string Filename;

        /// <summary>
        /// Gets if the opened file is a track from an audio CD.
        /// </summary>
        public bool IsAudioCd {
            get {
                string Extension = Path.GetExtension(this.Filename).ToLower();
                return Extension == ".cda";
            }
        }

		/// <summary>
		/// Initializes a new instance of <see cref="LocalAudioFilePlayback"/> using
		/// the given filename.
		/// </summary>
		/// <param name="Filename">The file to play.</param>
		public LocalAudioFilePlayback(string Filename)
			:base(Bass.BASS_StreamCreateFile(Filename, 0, 0, BASSFlag.BASS_DEFAULT)){
			this.Filename = Filename;

			if (this.SongInfo.Title == null) {
				this.songInfo = new SongInfo(this.songInfo) {
					Title = Path.GetFileNameWithoutExtension(Filename)
				};
			}
		}

		/// <summary>
		/// Gets the original file name of the media.
		/// </summary>
		public string OriginalFilename {
			get {
				return Path.GetFileName(this.Filename);
			}
		}

		/// <summary>
		/// Gets the original source (a local path or an URI) of the media.
		/// </summary>
		public string OriginalSource {
			get {
				return this.Filename;
			}
		}

		/// <summary>
		/// Gets the recommended file name of the media.
		/// </summary>
		public string RecommendedFilename {
			get {
				if (this.SongInfo.Title == null)
					return this.Filename.RemovePathInvalidChars();

				string Extension = Path.GetExtension(this.OriginalFilename);

				if (this.SongInfo.Artist != null)
					return $"{this.SongInfo.Artist} - {this.SongInfo.Title}{Extension}".RemovePathInvalidChars();
				
				string Result = $"{this.SongInfo.Title}{Extension}".RemovePathInvalidChars();

                return (this.IsAudioCd) ? Path.ChangeExtension(Result, "mp3") : Result;
			}
		}
	}
}
