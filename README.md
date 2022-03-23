# Store-CDN-Tool (32bit systems not supported)

## Licensed Under GPLv3

## Tool directions

**IMPORTENT**

**SQL Layout below for advanced users**

- Follow [The PS4 Store Github](https://github.com/LightningMods/PS4-Store/releases) For New Updates
- When an update is released Press the tools "Check for Store app Update" to get the newest Store Updates for the PS4 app
- You can download The [Package here (PKG-Zone)](https://pkg-zone.com/Store-R2.pkg), and Install then run it **ONLY Download from this Site**
- In the app go to Settings -> CDN and replace the CDN Url with `http://YOUR_LOCAL_PC_IP/` YOUR_LOCAL_PC_IP Is your PC LOCAL IP address and restart the app
- Add PKGs to the Database using one of the tools options
- Copy the PKG(s) you added to the database to the Tools "pkgs" folder and press "Start webserver" then relaunch the Store App

## Libaries


- [PS4_TOOLs](https://github.com/xXxTheDarkprogramerxXx/PS4_Tools/tree/master/PS4_Tools) 
- [Liborbispkg](https://github.com/maxton/LibOrbisPkg) 
## Refs
- [Google](google.com) 


 
## Store SQL Layout & Details

**Table name homebrews ALL values ARE STRINGS/VARCHAR**

```
pid = Primary ID
id = PKG TITLE ID
name = PKG/Game Name
desc = description line 1
image = URL to the Apps icon0
package = URL to the package
version = PKG Version
picpath = PS4 App pic path HAS to be the following /user/app/NPXS39041/storedata/PNG.png
desc_1 = description line 2
desc_2 = description line 3
ReviewStars = Stars (unused)
Size = PKG Size
Author = PKG Maker Default is "Store tool"
apptype = PKG Apptype Default is HB-Game
pv = PS4 FW VER
main_icon_path = URL to the Apps icon0
main_menu_pic = PS4 App pic path HAS to be the following /user/app/NPXS39041/storedata/PNG.png
releaseddate = Date 
number_downloads = Number of downloads for this App (unused)
```

## Languages

- The Store's Langs. repo is [HERE](https://github.com/LightningMods/Store-Languages)
- The Store uses the PS4's System software Lang setting


IF the settings file is loaded from USB all settings will be saved to the same USB

ONLY 1 apps can download at once using this tool, you can do up to 4 if you have more threads

## Official Discord server

Invite: https://discord.gg/GvzDdx9GTc

## Donations

We accept the following methods

- [Ko-fi](https://ko-fi.com/lightningmods)
- BTC: 3MEuZAaA7gfKxh9ai4UwYgHZr5DVWfR6Lw

if you donate and dont want to the message anymore created this folder after donating ``

## Credits

- [MODDED WARFARE](https://twitter.com/MODDED_WARFARE)


