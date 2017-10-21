using System;
using System.IO;
using System.Threading.Tasks;
using TomiSoft.MP3Player.Utils;

namespace TomiSoft.MP3Player.Playback {
	/// <summary>
	/// Represents a saveable media.
	/// </summary>
	public interface ISavable {
		/// <summary>
		/// Gets the original source (a local path or an URI) of the media.
		/// </summary>
		string OriginalSource {
			get;
		}

		/// <summary>
		/// Gets the original file name of the media.
		/// </summary>
		string OriginalFilename {
			get;
		}

		/// <summary>
		/// Gets the recommended file name of the media.
		/// </summary>
		string RecommendedFilename {
			get;
		}
	}
}
