﻿/* 
QuickRevert
Copyright 2016 Malah

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
*/

namespace QuickRevert {
	public class QBlizzyToolbar : QuickRevert {
	
		internal bool Enabled {
			get {
				return QSettings.Instance.BlizzyToolBar;
			}
		}
		private string TexturePath = QuickRevert.MOD + "/Textures/BlizzyToolBar";
		public static GameScenes[] AppScenes = {
			GameScenes.SPACECENTER
		};
		private void OnClick() { 
			QGUI.Instance.Settings ();
			Log ("OnClick", "QBlizzyToolbar");
		}

		private IButton Button;

		internal static bool isAvailable {
			get {
				return ToolbarManager.ToolbarAvailable && ToolbarManager.Instance != null;
			}
		}

		internal void Init() {
			if (!HighLogic.LoadedSceneIsGame || !isAvailable || !Enabled || Button != null) {
				return;
			}
			Button = ToolbarManager.Instance.add (MOD, MOD);
			Button.TexturePath = TexturePath;
			Button.ToolTip = MOD + ": Settings";
			Button.OnClick += (e) => OnClick ();
			Button.Visibility = new GameScenesVisibility(AppScenes);
			Log ("Init", "QBlizzyToolbar");
		}

		internal void Destroy() {
			if (!isAvailable || Button == null) {
				return;
			}
			Button.Destroy ();
			Button = null;
			Log ("Destroy", "QBlizzyToolbar");
		}

		internal void Reset() {
			if (Enabled) {
				Init ();
			} else {
				OnDestroy ();
			}
			Log ("Reset", "QBlizzyToolbar");
		}
	}
}