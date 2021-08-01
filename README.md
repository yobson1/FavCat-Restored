![gamer](https://github.com/FelkonEx/FavCat-Restored/blob/master/Untitled-2.jpg)

This repo's fork will contain changes to revert the decision of having VRC+ to use any of the favoriting within FavCat, forked from the [original VRCMods repo](https://github.com/knah/VRCMods).

**I highly recommend supporting Knah and the original VRCMods developers**, as they're the ones that set the foundation for FavCat.
And, if there are any issues, **submit an issue ticket and i'll have a look** 👀 or alternatively, [join my (new) discord](https://discord.com/invite/YWKpph8b3J) 😎

I haven't changed any of the functionality outside of breaking-off the FavCat's VRChat+ things from the base repo's commit, and i won't be unless:
* VRChat breaks the current version, and i need to fix it.
* There's some clean code changes in the original repo that clean up the existing FavCat.
* Merging in UIExpansionKit changes (for build purposes).

If you don't trust the releases i put up for whatever reason, `I suggest cloning this repo and building it yourself`.
It's always safer if you build it yourself or understand how the code works, since you know exactly what's going into the `.dll` files that are generated.
Hell, even decompile the .dll yourself and have a poke around, doesn't hurt to look.

## See [original VRCMods repo](https://github.com/knah/VRCMods/blob/master/README.md) for original README file
This one has just been yoinked and reverted.

## Canny Tickets
#### I recommend upvoting the Canny Tickets listed below, to show the VRChat team that these would be good additions to the game!

If you decide to comment on Canny, **be respectful** and **avoid mentioning modding** - the team still doesn't like it, obviously.  
And yes, it will probably be ignored/forever hanging in "under review" like the majority of Canny posts. At least we'll have a nice big upvote number on our ignored posts.

* [**Reconsider the approach to paywalling extra avatar favorite slots/groups**](https://feedback.vrchat.com/vrchat-plus-feedback/p/reconsider-the-approach-to-paywalling-extra-avatar-favorite-slotsgroups)
* [The ability to categorize avatars](https://feedback.vrchat.com/feature-requests/p/the-ability-to-categorize-avatars)
* [Search avatars](https://feedback.vrchat.com/feature-requests/p/search-avatars)
* [Personal avatar sorting](https://feedback.vrchat.com/feature-requests/p/personal-avatar-sorting)

## FavCat
An all-in-one local favorites mod. Unlimited favorite lists with unlimited favorites in them and a searchable local database of content and players.  
**Requires UI Expansion Kit 0.3.4 or newer**  
#### Features:
* Unlimited lists (categories) for favorites, each of unlimited size
* Lag-free even with large lists
* Freely changeable list height
* Avatar, world, and player favorites supported
* Modifiable list order and multiple list sorting options
* Fully searchable database of everything you have ever seen
* Changeable database location (**it's recommended to store the database in a directory backed up to cloud storage, such as Dropbox or OneDrive**, see below for setup)
* Local image cache for even better performance
* Categorize your own private avatars
* Import avatar favorites from other local favorite mods (read below)
* Exchange search database with friends (read below)
* Many more small things

#### Known limitations
* Player favorites don't show online status
* Lists with over a thousand elements can take a bit of time on game startup/list creation

#### Changing database location
Steps to change database location:
1. Run VRChat with the mod at least once
2. Make sure that VRChat is closed
3. Navigate to VRChat install directory (i.e. by clicking "Browse Local Files" in Steam)
4. Navigate to `UserData` folder and open `MelonPreferenes.cfg` with Notepad or other text editor
5. Find the line with `[FavCat]`
6. Find the line with `DatabasePath` under it
7. Change the value to absolute path to new storage folder. The new line should look like this: `DatabasePath = "C:/Users/username/OneDrive"` (with your own path, naturally; make sure to use forward clashes `/` instead of backslashes `\\`)
8. Save and close the text file
9. Copy the two (or four) database files (`favcat-favs.db` and `favacat-store.db`, and `favcat-favs-log.db` and `favcat-store-log.db` if they exist) from the old location (they are in `UserData` by default) to the new one.

If you want to move the image cache, use the same steps as above, but modify the line with `ImageCachePath` and copy `favcat-images.db` instead. It's not recommended to store the image cache in cloud storage due to its big size.

#### Importing avatar favorites from other local favorite mods
You can import favorites from other local favorite mods that use text files for storage.
1. Run VRChat with the mod at least once
2. Navigate to VRChat install directory (i.e. by clicking "Browse Local Files" in Steam)
3. Find the avatar list of the mod you want to import from. Places to look like are `UserData` folder, game folder, or a mod-specific folder, such as `404Mods`. The avatar list would usually be a plain text file or a JSON file - both are supported.
4. **Copy** the file to `UserData/FavCatImport` folder
5. In-game, click "More FavCat" on any big menu page, then click "Import databases and text files"
6. Import process can take some time. Once it is done, a new list will appear in the menu. It's safe to close the game and reopen it - import will continue from where it left off (you'll need to click the button again though). Once it is done, the corresponding list will be deleted from `UserData/FavCatImport` folder.

#### Sharing search database with friends
You can exchange the search database with friends to be able to find things they have seen. **Only accept databases from friends you trust - an intentionally malformed database can overwrite parts of yours with garbage**  
How to send database to a friend:
1. Run VRChat with the mod at least once (duh)
2. Make sure that VRChat is closed
3. Navigate to where your database is stored (see "Changing database location")
4. Make sure that there is no file named `favcat-store-log.db`. If there is one, it means that the game was not closed properly. In that case, run the game again, and use "Exit VRChat" button in settings menu to close it.
5. Send `favcat-store.db` to your friend.

How to receive database from a friend:
1. Run VRChat with the mod at least once
2. Navigate to VRChat install directory (i.e. by clicking "Browse Local Files" in Steam)
3. Put the database your friend sent you into `UserData/FavCatImport` folder. If you want to import multiple databases at once, you can rename them, as long as .db extension is kept.
4. In-game, click "More FavCat" on any big menu page, then click "Import databases and text files"
5. Import process can take some time. Once it is done, the corresponding database will be deleted from `UserData/FavCatImport` folder.

Note that your favorites are stored in `favcat-favs.db` - don't send it to your friends, favorite import is not supported. Most certainly don't send `favcat-images.db` to your friends - it's just a boring image cache.

#### Used libraries:
* [LiteDB](https://github.com/mbdavid/LiteDB) for all data storage
* [ImageSharp](https://github.com/SixLabors/ImageSharp), because unity is bad at loading images from streams on background thread

A long time ago this was based on Slaynash's [AvatarFav](https://github.com/Slaynash/AvatarFav) and [VRCTools](https://github.com/Slaynash/VRCTools), both licensed under the [MIT license](https://github.com/Slaynash/VRCTools/blob/master/LICENSE). Who knows how much of that still remains inside?

## UI Expansion Kit
This mod provides additional UI panels for use by other mods, and a unified mod settings UI.  
Some settings (currently boolean ones) can be pinned to quick menu for faster access.  
Refer to [API](UIExpansionKit/API) for mod integration.  
MirrorResolutionUnlimiter has an [example](MirrorResolutionUnlimiter/MirrorResolutionUnlimiterMod.cs) of soft dependency on this mod  
EmojiPageButtons has an [example](EmojiPageButtons/EmojiPageButtonsMod.cs) for delaying button creation until your mod is done
 
This mod uses [Google Noto](https://www.google.com/get/noto/) font, licensed under [SIL Open Font License 1.1](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL).  

## ILRepack
There's a copy of [ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task) and [ILRepack](https://github.com/gluck/il-repack) built for netcore/MSBuild 16 shipped with the repo.

## Installation
Before install:  
**Tupper (from VRChat Team) said that any modification of the game can lead to a ban, as with these mods**

To install these mods, you will need to install [MelonLoader](https://discord.gg/2Wn3N2P) (discord link, see \#how-to-install).  
Then, you will have to put mod .dll files in the `Mods` folder of your game directory

## Building
To build these, drop required libraries (found in `<vrchat instanll dir>/MelonLoader/Managed` after melonloader installation, list found in `Directory.Build.props`) into Libs folder, then use your IDE of choice to build.
 * Libs folder is intended for newest libraries (MelonLoader 0.4.3)

## License
With the following exceptions, all mods here are provided under the terms of [GNU GPLv3 license](LICENSE)
* ILRepack.Lib.MSBuild.Task is covered by [its own license](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task/blob/master/LICENSE.md)
* ILRepack is covered by [Apache 2.0 license](https://github.com/gluck/il-repack/blob/master/LICENSE)
* UI Expansion Kit is additionally covered by [LGPLv3](UIExpansionKit/COPYING.LESSER) to allow other mods to link to it
