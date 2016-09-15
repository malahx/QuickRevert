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

using UnityEngine;

namespace QuickRevert {

	public partial class QGUI {

		public static QGUI Instance;

		[KSPField(isPersistant = true)] private static QBlizzyToolbar BlizzyToolbar;

		private bool WindowSettings = false;
		private Rect rectSettings = new Rect();
		private Rect RectSettings {
			get {
				rectSettings.width = 515;
				rectSettings.x = (Screen.width - rectSettings.width) / 2;
				rectSettings.y = (Screen.height - rectSettings.height) / 2;
				return rectSettings;
			}
			set {
				rectSettings = value;
			}
		}

		protected override void Awake() {
			if (HighLogic.LoadedScene != GameScenes.SPACECENTER) {
				Warning ("QGUI needs to be load only in the SpaceCenter.", "QGUI");
				Destroy (this);
				return;
			}
			if (Instance != null) {
				Warning ("There's already an Instance of QGUI", "QGUI");
				Destroy (this);
				return;
			}
			Instance = this;
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			Log ("Awake", "QGUI");
		}

		protected override void Start() {
			BlizzyToolbar.Init ();
			if (!QFlight.data.hasLoaded) {
				QFlight.data.Load ();
			}
			Log ("Start", "QGUI");
		}

		protected override void OnDestroy() {
			BlizzyToolbar.Destroy ();
			Log ("OnDestroy", "QGUI");
		}

		private void Lock(bool activate, ControlTypes Ctrl) {
			if (HighLogic.LoadedSceneIsEditor) {
				if (activate) {
					EditorLogic.fetch.Lock(true, true, true, "EditorLock" + QuickRevert.MOD);
					return;
				} else {
					EditorLogic.fetch.Unlock ("EditorLock" + QuickRevert.MOD);
				}
			}
			if (activate) {
				InputLockManager.SetControlLock (Ctrl, "Lock" + QuickRevert.MOD);
				return;
			} else {
				InputLockManager.RemoveControlLock ("Lock" + QuickRevert.MOD);
			}
			if (InputLockManager.GetControlLock ("Lock" + QuickRevert.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("Lock" + QuickRevert.MOD);
			}
			if (InputLockManager.GetControlLock ("EditorLock" + QuickRevert.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("EditorLock" + QuickRevert.MOD);
			}
			Log ("Lock " + activate, "QGUI");
		}

		public void Settings() {
			SettingsSwitch ();
			if (!WindowSettings) {
				QStockToolbar.Instance.Reset();
				BlizzyToolbar.Reset();
				QSettings.Instance.Save ();
			}
			Log ("Settings", "QGUI");
		}

		private void SettingsSwitch() {
			WindowSettings = !WindowSettings;
			QStockToolbar.Instance.Set (WindowSettings);
			Lock (WindowSettings, ControlTypes.KSC_ALL);
			Log ("SettingsSwitch", "QGUI");
		}

		private void OnGUI() {
			if (!WindowSettings) {
				return;
			}
			GUI.skin = HighLogic.Skin;
			RectSettings = GUILayout.Window (1584652, RectSettings, DrawSettings, MOD + " " + VERSION);
		}

		private void DrawSettings(int id) {
			GUILayout.BeginVertical();
		
			if (QFlight.data.PostInitStateIsSaved) {
				if (QFlight.data.VesselExists) {
					GUILayout.BeginHorizontal();
					GUILayout.Box("Revert Saved",GUILayout.Height(30));
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					GUILayout.Label (string.Format("Revert of the last Vessel saved: <color=#FFFFFF><b>({0}){1}</b></color>", QFlight.data.pVessel.vesselType, QFlight.data.pVessel.vesselName), GUILayout.Width (400));
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Lose it")) {
						QFlight.data.Reset ();
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Goto this vessel")) {
						Settings ();
						string _saveGame = GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
						FlightDriver.StartAndFocusVessel (_saveGame, QFlight.data.currentActiveVesselIdx);
						InputLockManager.ClearControlLocks ();
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					if (!QFlight.data.PreLaunchStateIsSaved) {
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("<color=#FF0000><b>Only the revert to launch is saved!</b></color>", GUILayout.Width (500));
						GUILayout.EndHorizontal ();
						GUILayout.Space (5);
					}
				}
			}

			GUILayout.BeginHorizontal();
			GUILayout.Box("Options",GUILayout.Height(30));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			QSettings.Instance.EnableRevertLoss = GUILayout.Toggle (QSettings.Instance.EnableRevertLoss, "Enable the revert loss when you escape " + (Planetarium.fetch.Home.atmosphere ? "atmosphere" : "sphere of influence"), GUILayout.Width (250));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);

			GUILayout.BeginHorizontal ();
			QSettings.Instance.StockToolBar = GUILayout.Toggle (QSettings.Instance.StockToolBar, "Use the Stock ToolBar", GUILayout.Width (250));
			if (QBlizzyToolbar.isAvailable) {
				QSettings.Instance.BlizzyToolBar = GUILayout.Toggle (QSettings.Instance.BlizzyToolBar, "Use the Blizzy ToolBar", GUILayout.Width (250));
			}
			GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Close and Save",GUILayout.Height(30))) {
				Settings ();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space (5);
			GUILayout.EndVertical();
		}
	}
}