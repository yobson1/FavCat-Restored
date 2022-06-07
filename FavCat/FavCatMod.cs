using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using FavCat;
using FavCat.CustomLists;
using FavCat.Database;
using FavCat.Modules;
using HarmonyLib;
using MelonLoader;
using UIExpansionKit.API;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Networking;
using VRC.Core;
using VRC.UI;
using ImageDownloaderClosure = ImageDownloader.__c__DisplayClass11_0;
using Object = UnityEngine.Object;
using Newtonsoft.Json.Linq;

[assembly: MelonInfo(typeof(FavCatMod), "FavCatRestored", "1.1.19", "Felkon & yobson (Original By Knah)", "https://github.com/yobson1/FavCat-Restored")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("System.Text.Encoding.CodePages")]

namespace FavCat
{
	internal partial class FavCatMod : MelonMod
	{
		public static LocalStoreDatabase? Database;
		internal static FavCatMod Instance;

		internal AvatarModule? AvatarModule;
		internal WorldsModule? WorldsModule;
		internal PlayersModule? PlayerModule;

		internal static PageUserInfo PageUserInfo;

		private static bool IsVersionNewer(string version1, string version2)
		{
			var version1Parts = version1.Split('.');
			var version2Parts = version2.Split('.');

			for (var i = 0; i < Math.Min(version1Parts.Length, version2Parts.Length); i++)
			{
				if (int.Parse(version1Parts[i]) > int.Parse(version2Parts[i]))
				{
					return true;
				}
			}

			return false;
		}

		private static bool UpdateMod()
		{
			// Get our current version
			var currentVersion = Instance.Info.Version;

			try
			{
				// Get the latest version from the GitHub repo
				var webClient = new WebClient();
				webClient.Headers.Add("user-agent", "yobson1-FavCat-Restored");

				var releaseJson = webClient.DownloadString("https://api.github.com/repos/yobson1/FavCat-Restored/releases/latest");
				var release = JObject.Parse(releaseJson);
				string latestVersion = (string)release["tag_name"];

				if (latestVersion == null)
				{
					MelonLogger.Error("Failed to get latest version from GitHub");
					return false;
				}

				// If the current version is older than the latest version, we need to update
				if (IsVersionNewer(latestVersion, currentVersion))
				{
					MelonLogger.Msg($"Updating from v{currentVersion} to v{latestVersion}");
					webClient.DownloadFile("https://github.com/yobson1/FavCat-Restored/releases/latest/download/FavCatRestored.dll", Directory.GetCurrentDirectory() + "/Mods/FavCatRestored.dll");
					MelonLogger.Msg($"FavCatRestored has been updated to v{latestVersion}\nThe update will take effect on the next restart");
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Failed to get latest version from GitHub: {ex.Message}");
				return false;
			}
		}

		public override void OnApplicationStart()
		{
			Instance = this;
			if (!CheckWasSuccessful || !MustStayTrue || MustStayFalse) return;

			FavCatSettings.RegisterSettings();

			if (FavCatSettings.AutoUpdate.Value)
			{
				// TODO: Show a notification to the user there has been an update
				UpdateMod();
			}

			Directory.CreateDirectory("./UserData/FavCatImport");

			ClassInjector.RegisterTypeInIl2Cpp<CustomPickerList>();
			ClassInjector.RegisterTypeInIl2Cpp<CustomPicker>();

			ApiSnifferPatch.DoPatch();
			MelonLogger.Msg("Creating database");
			Database = new LocalStoreDatabase(FavCatSettings.DatabasePath.Value, FavCatSettings.ImageCachePath.Value);

			Database.ImageHandler.TrimCache(FavCatSettings.MaxCacheSizeBytes).NoAwait();

			foreach (var methodInfo in typeof(AvatarPedestal).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(it => it.Name.StartsWith("Method_Private_Void_ApiContainer_") && it.GetParameters().Length == 1))
			{
				HarmonyInstance.Patch(methodInfo, new HarmonyMethod(typeof(FavCatMod), nameof(AvatarPedestalPatch)));
			}

			DoAfterUiManagerInit(OnUiManagerInit);
		}

		private static void AvatarPedestalPatch(ApiContainer __0)
		{
			if (__0.Code != 200) return;
			var model = __0.Model?.TryCast<ApiAvatar>();
			if (model == null) return;

			if (MelonDebug.IsEnabled())
				MelonDebug.Msg($"Ingested avatar with ID={model.id}");
			Database?.UpdateStoredAvatar(model);
		}

		internal CustomPickerList CreateCustomList(Transform parent)
		{
			var go = Object.Instantiate(AssetsHandler.ListPrefab, parent);
			go.SetActive(true);
			return go.AddComponent<CustomPickerList>();
		}

		public override void OnApplicationQuit()
		{
			Database?.Dispose();
			Database = null;
		}

		private static IEnumerator HideVRCPlusCategoriesCoro()
		{
			var avatarLists = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/FavoriteContent").transform;
			while (avatarLists.Find("avatars1") == null)
				yield return null;

			HideVRCPlusCategories(avatarLists);
		}

		internal static void HideVRCPlusCategories(Transform avatarLists)
		{
			MelonLogger.Msg("Hiding VRC+ categories");
			Object.DestroyImmediate(avatarLists.Find("avatars2").gameObject);
			Object.DestroyImmediate(avatarLists.Find("avatars3").gameObject);
			Object.DestroyImmediate(avatarLists.Find("avatars4").gameObject);
			Object.DestroyImmediate(avatarLists.Find("avatars1/GetMoreFavorites").gameObject);
		}

		public void OnUiManagerInit()
		{
			AssetsHandler.Load();

			try
			{
				if (FavCatSettings.EnableAvatarFavs.Value)
					AvatarModule = new AvatarModule();
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Exception in avatar module init: {ex}");
			}

			try
			{
				if (FavCatSettings.EnableWorldFavs.Value)
					WorldsModule = new WorldsModule();
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Exception in world module init: {ex}");
			}

			try
			{
				if (FavCatSettings.EnablePlayerFavs.Value)
					PlayerModule = new PlayersModule();
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Exception in player module init: {ex}");
			}

			PageUserInfo = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
			MelonLogger.Msg("Initialized!");

			if (FavCatSettings.HideVRCPlusCategories.Value)
				MelonCoroutines.Start(HideVRCPlusCategoriesCoro());
		}

		public override void OnUpdate()
		{
			AvatarModule?.Update();
			WorldsModule?.Update();
			PlayerModule?.Update();
			GlobalImageCache.OnUpdate();
		}
	}

	public class ApiSnifferPatch
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ApiPopulateDelegate(IntPtr @this, IntPtr dictionary, IntPtr someRef, IntPtr methodRef);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ImageDownloaderOnDoneDelegate(IntPtr thisPtr, IntPtr asyncOperationPtr, IntPtr methodInfo);

		private static ApiPopulateDelegate ourOriginalApiPopulate = (_, _, _, _) => 0;
		private static ImageDownloaderOnDoneDelegate ourOriginalOnDone = (_, _, _) => { };

		private static readonly Type ImageDownloaderClosureType;
		private static readonly MethodInfo WebRequestField;
		private static readonly MethodInfo ImageUrlField;
		private static readonly MethodInfo? NestedClosureField;

		static ApiSnifferPatch()
		{
			ImageDownloaderClosureType = typeof(ImageDownloader).GetNestedTypes().Single(it => it.GetMethod(nameof(ImageDownloaderClosure._DownloadImageInternal_b__0)) != null);
			WebRequestField = ImageDownloaderClosureType.GetProperties().Single(it => it.PropertyType == typeof(UnityWebRequest)).GetMethod;
			NestedClosureField = ImageDownloaderClosureType.GetProperties().SingleOrDefault(it => it.PropertyType.IsNested && it.PropertyType.DeclaringType == typeof(ImageDownloader))?.GetMethod;
			Type? possibleNestedClosureType = NestedClosureField?.ReturnType;
			ImageUrlField = (NestedClosureField != null ? possibleNestedClosureType : ImageDownloaderClosureType)!.GetProperty("imageUrl")!.GetMethod;
		}

		public static void DoPatch()
		{
			NativePatchUtils.NativePatch(typeof(ApiModel).GetMethods().Single(it =>
					it.Name == nameof(ApiModel.SetApiFieldsFromJson) && it.GetParameters().Length == 2 && it.GetParameters()[0].ParameterType.GenericTypeArguments[1] != typeof(Il2CppSystem.Object)),
				out ourOriginalApiPopulate, ApiSnifferStatic);

			NativePatchUtils.NativePatch(ImageDownloaderClosureType.GetMethod(nameof(ImageDownloaderClosure
				._DownloadImageInternal_b__0))!, out ourOriginalOnDone, ImageSnifferPatch);
		}

		private static readonly object[] EmptyObjectArray = new object[0];

		public static void ImageSnifferPatch(IntPtr instancePtr, IntPtr asyncOperationPtr, IntPtr methodInfo)
		{
			ourOriginalOnDone(instancePtr, asyncOperationPtr, methodInfo);

			try
			{
				if (!FavCatSettings.UseLocalImageCache || FavCatMod.Database == null)
					return;

				var closure = Activator.CreateInstance(ImageDownloaderClosureType, instancePtr);
				var url = (string)ImageUrlField.Invoke(NestedClosureField?.Invoke(closure, EmptyObjectArray) ?? closure, EmptyObjectArray);

				var webRequest = (UnityWebRequest)WebRequestField.Invoke(closure, EmptyObjectArray);
				if (webRequest.isNetworkError || webRequest.isHttpError)
					return;

				if (webRequest.downloadedBytes > 1024 * 1024)
				{
					if (MelonDebug.IsEnabled())
						MelonDebug.Msg($"Ignored downloaded image from {url} because it's bigger than 1 MB");
					return; // ignore images over 1 megabyte, 256-pixel previews should not be that big
				}

				FavCatMod.Database.ImageHandler.StoreImageAsync(url, webRequest.downloadHandler.data).NoAwait();
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Exception in image downloader patch: {ex}");
			}
		}

		public static byte ApiSnifferStatic(IntPtr @this, IntPtr dictionary, IntPtr someRef, IntPtr methodInfo)
		{
			var result = ourOriginalApiPopulate(@this, dictionary, someRef, methodInfo);

			try
			{
				var apiModel = new ApiModel(@this);

				if (apiModel.Populated)
				{
					if (apiModel.Endpoint == "worlds")
					{
						var world = apiModel.Cast<ApiWorld>();
						FavCatMod.Database?.UpdateStoredWorld(world);
					}
					else if (apiModel.Endpoint == "users")
					{
						var user = apiModel.Cast<APIUser>();
						FavCatMod.Database?.UpdateStoredPlayer(user);
					}
					else if (apiModel.Endpoint == "avatars")
					{
						var avatar = apiModel.Cast<ApiAvatar>();
						FavCatMod.Database?.UpdateStoredAvatar(avatar);
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"Exception in API sniffer patch: {ex}");
			}

			return result;
		}
	}
}
