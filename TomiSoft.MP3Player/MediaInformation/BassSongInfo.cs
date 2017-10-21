using System;
using System.Drawing;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;

namespace TomiSoft.MP3Player.MediaInformation {
	/// <summary>
	/// Provides information of a song using BassTag plugin.
	/// </summary>
	public class BassSongInfo : ISongInfo {
		/// <summary>
		/// Stores the metadatas of the song.
		/// </summary>
		private TAG_INFO tagInfo;

		/// <summary>
		/// Gets the album name of the song.
		/// </summary>
		public string Album {
			get {
				return String.IsNullOrWhiteSpace(tagInfo.album) ? null : tagInfo.album;
			}
		}

		/// <summary>
		/// Gets the artist of the song.
		/// </summary>
		public string Artist {
			get {
				if (!String.IsNullOrWhiteSpace(tagInfo.artist))
					return tagInfo.artist;

				if (!String.IsNullOrWhiteSpace(tagInfo.albumartist))
					return tagInfo.albumartist;

				return null;
			}
		}

		/// <summary>
		/// Gets the title of the song.
		/// </summary>
		public string Title {
			get {
				return String.IsNullOrWhiteSpace(tagInfo.title) ? null : tagInfo.title;
			}
		}

		/// <summary>
		/// Gets the length of the song in seconds.
		/// </summary>
		public double Length {
			get {
				return this.tagInfo.duration;
			}
		}

		/// <summary>
		/// Returns true if the Length is represented in seconds,
		/// false if the Length is represented in bytes.
		/// </summary>
		public bool IsLengthInSeconds {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets the file's path or URI.
		/// </summary>
		public string Source {
			get {
				return this.tagInfo.filename;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BassSongInfo"/> class.
		/// </summary>
		private BassSongInfo() {
			BassTags.ReadPictureTAGs = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BassSongInfo"/> class.
		/// </summary>
		/// <param name="Path">The file's path to read</param>
		public BassSongInfo(string Path) : this() {
			#region Error checking
			if (!File.Exists(Path))
				throw new FileNotFoundException($"{Path} does not exists.", Path);
			#endregion

			this.tagInfo = BassTags.BASS_TAG_GetFromFile(Path);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BassSongInfo"/> class.
		/// </summary>
		/// <param name="ChannelID">The BASS handle of the channel</param>
		/// <exception cref="ArgumentException">when ChannelID is 0</exception>
		public BassSongInfo(int ChannelID) : this() {
			#region Error checking
			if (ChannelID == 0)
				throw new ArgumentException($"{nameof(ChannelID)} cannot be 0.");
			#endregion

			this.tagInfo = new TAG_INFO();
			BassTags.BASS_TAG_GetFromFile(ChannelID, this.tagInfo);
		}
	}
}
