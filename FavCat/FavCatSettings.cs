using LiteDB;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

#nullable disable

namespace FavCat
{
	public static class FavCatSettings
	{
		private const string SettingsCategory = "FavCat";
		private static MelonPreferences_Category Category;

		internal static MelonPreferences_Entry<string> DatabasePath;
		internal static MelonPreferences_Entry<string> ImageCachePath;

		internal static MelonPreferences_Entry<bool> EnableAvatarFavs;
		internal static MelonPreferences_Entry<bool> EnableWorldFavs;
		internal static MelonPreferences_Entry<bool> EnablePlayerFavs;

		private static MelonPreferences_Entry<string> ImageCacheMode;
		private static MelonPreferences_Entry<int> ImageCacheMaxSize;
		internal static MelonPreferences_Entry<bool> HidePopupAfterFav;

		internal static MelonPreferences_Entry<bool> MakeClickSounds;
		internal static MelonPreferences_Entry<string> AvatarSearchMode;
		internal static MelonPreferences_Entry<bool> SortPlayersByOnline;

		internal static MelonPreferences_Entry<bool> SortPlayersByJoinable;

		internal static MelonPreferences_Entry<bool> UseCustomStyles;
		internal static MelonPreferences_Entry<bool> ColorBackground;
		internal static MelonPreferences_Entry<bool> UseStyletorColors;
		internal static MelonPreferences_Entry<string> StyletorBase;
		internal static MelonPreferences_Entry<string> StyletorAccent;

		internal static MelonPreferences_Entry<string> BaseColorPref;
		internal static MelonPreferences_Entry<string> AccentColorPref;
		internal static MelonPreferences_Entry<bool> HideVRCPlusCategories;

		internal static MelonPreferences_Entry<bool> AutoUpdate;

		private static bool IsStyletorLoaded
		{
			get
			{
				return MelonHandler.Mods.Any(x => x.Info.Name == "Styletor");
			}
		}

		private static bool UseStyletor
		{
			get
			{
				return UseCustomStyles.Value && IsStyletorLoaded && UseStyletorColors.Value;
			}
		}

		private static IEnumerator RegisterStyletorSettings()
		{
			// We need to wait so that Styletor can load before us
			yield return new WaitForEndOfFrame();
			MelonLogger.Msg("Styletor is loaded, adding Styletor settings");
			UseStyletorColors = Category.CreateEntry(nameof(UseStyletorColors), false, "Inherit color choices from Styletor");
			StyletorBase = MelonPreferences.GetEntry<string>("Styletor", "BaseColorString");
			StyletorAccent = MelonPreferences.GetEntry<string>("Styletor", "AccentColorString");
		}

		private static void HideVRCPlusCategoriesChanged(bool oldVal, bool newVal)
		{
			if (newVal && GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/FavoriteContent/avatars2") != null)
			{
				FavCatMod.HideVRCPlusCategories(GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/FavoriteContent").transform);
			}
		}

		internal static void RegisterSettings()
		{
			var avatarSearchModeName = "AvatarSearchMode";
			Category = MelonPreferences.CreateCategory(SettingsCategory, "FavCat");

			DatabasePath = Category.CreateEntry("DatabasePath", "./UserData", "Database directory path", is_hidden: true, dont_save_default: false);
			ImageCachePath = Category.CreateEntry("ImageCachePath", "./UserData", "Image cache directory path", is_hidden: true, dont_save_default: false);

			EnableAvatarFavs = Category.CreateEntry("EnableAvatarFavs", true, "Enable avatar favorites (restart required)");
			EnableWorldFavs = Category.CreateEntry("EnableWorldFavs", true, "Enable world favorites (restart required)");
			EnablePlayerFavs = Category.CreateEntry("EnablePlayerFavs", true, "Enable player favorites (restart required)");

			ImageCacheMode = Category.CreateEntry("ImageCachingMode", "full", "Image caching mode");
			ImageCacheMaxSize = Category.CreateEntry("ImageCacheMaxSize", 4096, "Image cache max size (MB)");
			HidePopupAfterFav = Category.CreateEntry("HidePopupAfterFav", true, "Hide favorite popup after (un)favoriting a world or a player");

			MakeClickSounds = Category.CreateEntry("MakeClickSounds", true, "Click sounds");
			AvatarSearchMode = Category.CreateEntry(avatarSearchModeName, "select", "Avatar search result action");
			SortPlayersByOnline = Category.CreateEntry(nameof(SortPlayersByOnline), true, "Show offline players at the end of the list");

			SortPlayersByJoinable = Category.CreateEntry(nameof(SortPlayersByJoinable), true, "Show players in private instances at the end of the list");

			UseCustomStyles = Category.CreateEntry(nameof(UseCustomStyles), true, "Use custom styles (requires restart to disable)");
			ColorBackground = Category.CreateEntry(nameof(ColorBackground), false, "Use custom colors on the background of favorite lists");
			if (IsStyletorLoaded)
			{
				MelonCoroutines.Start(RegisterStyletorSettings());
			}

			BaseColorPref = Category.CreateEntry(nameof(BaseColorPref), "0 60 60", "Base color");
			AccentColorPref = Category.CreateEntry(nameof(AccentColorPref), "106 227 249", "Accent color");

			HideVRCPlusCategories = Category.CreateEntry(nameof(HideVRCPlusCategories), false, "Hide VRC+ categories");
			HideVRCPlusCategories.OnValueChanged += HideVRCPlusCategoriesChanged;

			AutoUpdate = Category.CreateEntry(nameof(AutoUpdate), true, "Auto update FavCat");

			ExpansionKitApi.RegisterSettingAsStringEnum(SettingsCategory, "ImageCachingMode", new[] { ("full", "Full local image cache (fastest, safest)"), ("fast", "Fast, use more RAM"), ("builtin", "Preserve RAM, more API requests") });
			ExpansionKitApi.RegisterSettingAsStringEnum(SettingsCategory, avatarSearchModeName, new[] { ("select", "Select avatar"), ("author", "Show avatar author (safer)") });
		}

		// https://github.com/knah/VRCMods/blob/6c5badbcaebdab3fd9c5fbf2fc1467d56c127688/Styletor/SettingsHolder.cs#L178
		private static int ParseComponent(string[] split, int idx, int defaultValue = 255)
		{
			if (split.Length <= idx || !int.TryParse(split[idx], out var parsed)) parsed = defaultValue;
			if (parsed < 0) parsed = 0;
			else if (parsed > 255) parsed = 255;
			return parsed;
		}

		private static Color ParseColor(string str)
		{
			var split = str.Split(' ');
			var r = ParseComponent(split, 0, 200);
			var g = ParseComponent(split, 1, 200);
			var b = ParseComponent(split, 2, 200);
			var a = ParseComponent(split, 3, 255);

			return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		internal static ColorBlock ButtonColors
		{
			get
			{
				Color baseColor;
				Color accentColor;
				if (UseStyletor)
				{
					baseColor = ParseColor(StyletorBase.Value);
					accentColor = ParseColor(StyletorAccent.Value);
				}
				else
				{
					baseColor = ParseColor(BaseColorPref.Value);
					accentColor = ParseColor(AccentColorPref.Value);
				}

				return new ColorBlock
				{
					normalColor = baseColor,
					highlightedColor = accentColor,
					pressedColor = accentColor.RGBMultiplied(0.8f),
					disabledColor = baseColor.RGBMultiplied(0.3f),
					colorMultiplier = 1
				};
			}
		}

		internal static Color BaseColor
		{
			get
			{
				return UseStyletor ? ParseColor(StyletorBase.Value) : ParseColor(BaseColorPref.Value);
			}
		}

		internal static Color AccentColor
		{
			get
			{
				return UseStyletor ? ParseColor(StyletorAccent.Value) : ParseColor(AccentColorPref.Value);
			}
		}

		public static bool UseLocalImageCache => ImageCacheMode.Value == "full";
		public static bool CacheImagesInMemory => ImageCacheMode.Value == "fast";
		public static long MaxCacheSizeBytes => ImageCacheMaxSize.Value * 1024L * 1024L;


	}
}
