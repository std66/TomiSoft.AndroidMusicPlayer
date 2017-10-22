using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using Un4seen.Bass;
using TomiSoft.MP3Player.Utils.Extensions;
using Android.Content.PM;

namespace TomiSoft.MP3Player.Playback.BASS {
	/// <summary>
	/// This class manages the BASS library.
	/// </summary>
	public static class BassManager {
		/// <summary>
		/// Stores the supported extensions by BASS and its plugins.
		/// </summary>
		private static readonly List<string> SupportedExtensions = new List<string>();

		/// <summary>
		/// Stores the loaded BASS plugins' name and the associated handle to it.
		/// </summary>
		private static readonly Stack<KeyValuePair<string, int>> LoadedPlugins = new Stack<KeyValuePair<string, int>>();

		/// <summary>
		/// Stores the BASS plugins' file names that should be bundled with the application.
		/// </summary>
		private static readonly string[] Plugins = {
			"libbassflac.so",
			"libbassopus.so",
			"libtags.so"
		};

		/// <summary>
		/// Gets if BASS is loaded
		/// </summary>
		public static bool BassLoaded {
			get;
			private set;
		} = false;

		/// <summary>
		/// Gets if BASS is initialized.
		/// </summary>
		public static bool BassInitialized {
			get;
			private set;
		} = false;

		/// <summary>
		/// Gets if all BASS plugins has been loaded successfully.
		/// </summary>
		public static bool AllPluginsLoaded {
			get;
			private set;
		} = false;

		/// <summary>
		/// Gets the version of the BASS library.
		/// </summary>
		/// <exception cref="InvalidOperationException">when BassLoaded is false</exception>
		public static Version BassVersion {
			get {
				#region Error checking
				if (!BassLoaded) {
					Trace.TraceWarning($"Cannot get {nameof(BassVersion)} because BASS is not loaded yet.");
					throw new InvalidOperationException("BASS must be loaded first.");
				}
				#endregion

				return Bass.BASS_GetVersion(4);
			}
		}

		/// <summary>
		/// Loads BASS and all of its plugins. BASS DLLs must be located in the directory
		/// \Bass\x64 and \Bass\x86.
		/// </summary>
		/// <param name="PluginDirectory">The directory containing the BASS plugins.</param>
		/// <returns>True if BASS is successfully loaded, false if not.</returns>
		public static bool Load(string PluginDirectory) {
			#region Error checking
			if (BassLoaded) {
				Trace.TraceInformation("There's no need to load BASS library again because it is already loaded.");
				return true;
			}
			#endregion

			BassNet.Registration("sinkutamas@gmail.com", "2X28292820152222");
			
			LoadBassPlugins(PluginDirectory);

			if (!LoadBass())
				return false;

			return true;
		}

		/// <summary>
		/// Initializes or reinitializes BASS output device.
		/// </summary>
		/// <returns>True if initialization was successful, false if not</returns>
		public static bool InitializeOutputDevice() {
			Trace.TraceInformation($"[BASS output init] Initializing BASS (Reinitialize={BassInitialized})");

			#region Error checking
			if (!BassLoaded) {
				Trace.TraceError($"[BASS output init] Cannot initialize BASS because it is not loaded. (BassLoaded={BassLoaded})");
				return false;
			}
			#endregion

			if (BassInitialized) {
				if (!Bass.BASS_Free()) {
					Trace.TraceError($"[BASS output init] Cannot release BASS for reinitializing: Bass.BASS_Free returned false (BassErrorCode={Bass.BASS_ErrorGetCode().ToString()})");
					return false;
				}
				else {
					Trace.TraceInformation("[BASS output init] BASS released for reinitializing.");
					BassInitialized = false;
				}
			}

			if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)) {
				Trace.TraceError($"[BASS output init] Cannot initialize BASS: (BassErrorCode={Bass.BASS_ErrorGetCode().ToString()})");
				return false;
			}

			Trace.TraceInformation("[BASS output init] BASS initialized successfully.");

			BassInitialized = true;
			return true;
		}

		/// <summary>
		/// Gets all supported file extensions that is supported by BASS.
		/// </summary>
		/// <returns>A collection that contains the supported file extensions (without the prepending dot).</returns>
		public static IEnumerable<string> GetSupportedExtensions() {
			return SupportedExtensions;
		}

		/// <summary>
		/// Determines whether the given file is supported by BASS.
		/// </summary>
		/// <param name="Filename">The path of the file to check</param>
		/// <returns>
		/// True if the file is supported. False is returned if not, or BASS is not loaded yet, or the
		/// given file does not exists.
		/// </returns>
		public static bool IsSupportedFile(string Filename) {
			#region Error checking
			if (!BassLoaded || !File.Exists(Filename))
				return false;
			#endregion

			string Extension = new FileInfo(Filename).Extension.Substring(1).ToLower();
			return SupportedExtensions.Contains(Extension);
		}

		/// <summary>
		/// Loads BASS library from the given directory.
		/// </summary>
		/// <returns>True if BASS is successfully loaded, false if not.</returns>
		private static bool LoadBass() {
			SupportedExtensions.AddRange(
				Bass.SupportedStreamExtensions.GetMatches(@"\W+([\w\d]+)").Select(x => x.ToLower())
			);

			BassLoaded = true;

			return true;
		}

		/// <summary>
		/// Loads all BASS plugins from the given directory.
		/// </summary>
		/// <param name="Directory">The directory that contains the plugin DLLs.</param>
		private static void LoadBassPlugins(string Directory) {
			AllPluginsLoaded = true;
			foreach (var Filename in Plugins) {
				#region Error checking
				if (!File.Exists(Directory + Filename)) {
					Trace.TraceWarning($"[BASS init] Plugin file not found ({Directory + Filename})");
					continue;
				}
				#endregion
				
				int Result = Bass.BASS_PluginLoad(Directory + Filename);

				if (Result != 0) {
					Trace.TraceInformation($"[BASS init] Plugin loaded: {Filename}");

					string PluginSupportedExtensions = Un4seen.Bass.Utils.BASSAddOnGetSupportedFileExtensions(Directory + Filename);
					SupportedExtensions.AddRange(
						PluginSupportedExtensions.GetMatches(@"\W+([\w\d]+)").Select(x => x.ToLower())
					);

					LoadedPlugins.Push(new KeyValuePair<string, int>(Filename, Result));
				}
				else {
					Trace.TraceWarning($"[BASS init] Failed to load plugin: {Filename} (Code {Result}, BassError={Bass.BASS_ErrorGetCode().ToString()})");
					AllPluginsLoaded = false;
				}
			}
		}

		/// <summary>
		/// Releases BASS and all of its reaources.
		/// </summary>
		public static void Free() {
			Trace.TraceInformation("[BASS cleanup] Releasing associated resources...");

			if (FreePlugins()) {
				Trace.TraceInformation("[BASS cleanup] All plugins released successfully.");
			}
			else {
				Trace.TraceWarning("[BASS cleanup] Failed to release all plugins.");
			}

			if (Bass.BASS_Free()) {
				Trace.TraceInformation("[BASS cleanup] BASS released successfully.");
			}
			else {
				Trace.TraceWarning("[BASS cleanup] Could not release BASS.");
			}
		}

		/// <summary>
		/// Releases all previously loaded BASS plugins.
		/// </summary>
		/// <returns>True if all plugins were released successfully, false if not.</returns>
		private static bool FreePlugins() {
			bool Result = true;

			while (LoadedPlugins.Count > 0) {
				KeyValuePair<string, int> Plugin = LoadedPlugins.Pop();
				
				if (!Bass.BASS_PluginFree(Plugin.Value)) {
					Trace.TraceWarning($"[BASS cleanup] Could not release plugin (Plugin: {Plugin.Key}, Handle: {Plugin.Value})");
					Result = false;
				}
			}

			return Result;
		}
	}
}