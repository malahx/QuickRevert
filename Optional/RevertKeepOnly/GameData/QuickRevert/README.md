﻿#### QuickRevert
#### A plugin for Kerbal Space Program 1.0.X
#### Copyright 2015 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 


#### What is it?

QuickRevert is a small plugin which adds the possibility to keep/pay the revert function.

#### How does it work?

You will lose the revert function if:
* you launch a new vessel,
* you stand too long (15 mins) outside the vessel (space center, tracking station, another vessel ...), it can be disabled,
* you can't pay a revert (only for career), it can be disabled.

You will keep the revert function if:
* you go to the space center or on another vessel,
* you make a quickload,
* your KSP crash,
* you stop your game,
* your launched vessel is loaded,
* your launched vessel is in prelaunch.
* you are on EVA,

#### How to install it?

Unzip all files. Put the QuickRevert folder in your KSP/GameData folder.

#### How to update it?

Unzip all files. Merge the new QuickRevert folder with the old folder which is in your KSP/GameData folder.

#### How to uninstall it?

Delete the QuickRevert folder in your KSP/GameData folder.

#### Changelog

v2.12 - 2015.11.24
* Fix: Compiled against KSP 1.0.5

v2.11 - 2015.06.26
* Fix: Deleted the debug warning.

v2.10 - 2015.06.25
* New: Added an option to change the minimal price of a revert,
* New: Added a maximal price of a revert (by default 200% of the cost of a revert),
* New: Added an auto hide the stock toolbar if the revert is disabled for the current game,
* New: Added a button to go to the revert's saved vessel from the Space Center,
* Fix: Corrected the Stock Toolbar,
* Fix: Corrected the lose of the revert after a restart/crash of KSP,
* Fix: Corrected the lose of the revert if the revert to editor is not saved,
* Fix: Corrected the revert price when it is influenced by the VesselCost,
* Fix: Corrected the cost of a revert to editor after many revert to launch,
* Fix: Corrected the load of the revert after a switch of the vessel,
* Fix: Corrected the reset of the revert after a recover of the vessel,
* Fix: Changed the default minimal price to 50% of the cost of a revert,
* Fix: Changed the default reputation price to 5,
* Fix: Changed the default science price to 1,
* Fix: Some minor bug.

v2.00 - 2015.05.04
* New: Added the revert cost,
* New: Added a GUI with stock and blizzy toolbar support,
* New: Added a config file,
* Fix: Converted textures to DDS,
* Fix: Corrected the Stock Toolbar icon not being created,
* Fix: Deleted the save of useless things as the vessel guid, the time and the flightstatecache,
* Fix: Rewrited the code and fix some minor bug,
* Optional: Added an optional version of QuickRevert with only the revert keeper,
* Optional: Added an optional version of QuickRevert with only the revert cost,
* Updated ToolbarWrapper to 1.7.9
* Updated to KSP 1.02

v1.11 - 2014.12.19
* Fix: Corrected the revert to editor on the launchpad,
* Fix: Old flight state will be remove to avoid errors,
* Fix: Flight state will be saved on the QuickRevert folder,
* Fix: Some minor bug.
* Updated to KSP 0.90

v1.10 - 2014.10.08
* New: Added a delay to keep the revert function while you are outside the vessel.
* New: Added the revert function after a KSP crash,
* Fix: Some minor bug,
* Updated to KSP 0.25

v1.00 - 2014.10.01
* Initial release

#### Thanks!

* to Matthieu James for the Faenza icon theme,
* to all mods developers which make this game really huge,
* to my friend Neimad who corrects my bad english ...
* to Squad for this awesome game.

#### Links

* http://forum.kerbalspaceprogram.com/threads/95168#QuickRevert
* https://kerbalstuff.com/mod/237
* http://kerbal.curseforge.com/ksp-mods/224621
* https://github.com/malahx/QuickRevert
* Faenza icon theme: http://gnome-look.org/content/show.php/Faenza?content=128143
