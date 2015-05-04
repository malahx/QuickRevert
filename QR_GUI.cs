/* 
QuickRevert
Copyright 2015 Malah

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

using System;
using UnityEngine;

namespace QuickRevert {

	public class QGUI : MonoBehaviour {

		internal static bool WindowSettings = false;
		private static Rect RectSettings = new Rect();
		private static GUIStyle GUIStyleSettingsLabel;
		private static GUIStyle GUIStyleSettingsTextField;

		private static float text;
		private static string TimeToKeep;
		private static string CreditsCost;
		private static string ReputationsCost;
		private static string SciencesCost;
		private static string VesselBasePrice;
		private static string RevertToLaunchFactor;

		internal static void Awake() {
			if (!HighLogic.LoadedSceneIsGame) {
				return;
			}
			text = QCareer.VesselCost;
			TimeToKeep = (QSettings.Instance.TimeToKeep / 60).ToString();
			CreditsCost = QSettings.Instance.CreditsCost.ToString();
			ReputationsCost = QSettings.Instance.ReputationsCost.ToString();
			SciencesCost = QSettings.Instance.SciencesCost.ToString();
			VesselBasePrice = QSettings.Instance.VesselBasePrice.ToString();
			RevertToLaunchFactor = (QSettings.Instance.RevertToLaunchFactor * 100).ToString();
			GUIStyleSettingsLabel = new GUIStyle (HighLogic.Skin.label);
			GUIStyleSettingsLabel.stretchWidth = true;
			GUIStyleSettingsLabel.stretchHeight = false;
			GUIStyleSettingsLabel.alignment = TextAnchor.UpperLeft;
			GUIStyleSettingsLabel.fontStyle = FontStyle.Normal;

			GUIStyleSettingsTextField = new GUIStyle (HighLogic.Skin.textField);
			GUIStyleSettingsTextField.alignment = TextAnchor.MiddleCenter;

			RectSettings = new Rect ((Screen.width - 515) / 2, 40, 515, 0);
		}

		private static void Lock(bool activate, ControlTypes Ctrl) {
			if (HighLogic.LoadedSceneIsEditor) {
				if (activate) {
					EditorLogic.fetch.Lock(true, true, true, "EditorLock" + Quick.MOD);
					return;
				} else {
					EditorLogic.fetch.Unlock ("EditorLock" + Quick.MOD);
				}
			}
			if (activate) {
				InputLockManager.SetControlLock (Ctrl, "Lock" + Quick.MOD);
				return;
			} else {
				InputLockManager.RemoveControlLock ("Lock" + Quick.MOD);
			}
			if (InputLockManager.GetControlLock ("Lock" + Quick.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("Lock" + Quick.MOD);
			}
			if (InputLockManager.GetControlLock ("EditorLock" + Quick.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("EditorLock" + Quick.MOD);
			}
		}

		public static void Settings() {
			SettingsSwitch ();
			if (!WindowSettings) {
				QuickRevert.StockToolbar.Reset();
				QuickRevert.BlizzyToolbar.Reset();
				QSettings.Instance.Save ();
			}
		}

		internal static void SettingsSwitch() {
			WindowSettings = !WindowSettings;
			QuickRevert.StockToolbar.Set (WindowSettings);
			Lock (WindowSettings, ControlTypes.KSC_ALL);
		}

		internal static void OnGUI() {
			if (WindowSettings) {
				GUI.skin = HighLogic.Skin;
				RectSettings = GUILayout.Window (1584652, RectSettings, DrawSettings, Quick.MOD + " " + Quick.VERSION, GUILayout.Width (RectSettings.width), GUILayout.ExpandHeight(true));
			}
		}

		private static void DrawSettings(int id) {
			int _int;
			float _float;
			bool _bool;
			GUILayout.BeginVertical();

			if (QFlight.data.isSaved) {
				if (QFlight.data.VesselExists) {
					GUILayout.BeginHorizontal();
					GUILayout.Box("Revert Saved",GUILayout.Height(30));
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					GUILayout.Label (string.Format("Revert of the last Vessel saved: ({0}){1}", QFlight.data.pVessel.vesselType, QFlight.data.pVessel.vesselName), GUIStyleSettingsLabel, GUILayout.Width (250));
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button ("Lose the last revert",GUILayout.Height(30))) {
						QFlight.data.Reset ();
						RectSettings.height = 0;
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
				}
			}

			GUILayout.BeginHorizontal();
			GUILayout.Box("Options",GUILayout.Height(30));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			_bool = GUILayout.Toggle (QSettings.Instance.EnableRevertLoss, "Enable the revert loss", GUILayout.Width (250));
			if (_bool != QSettings.Instance.EnableRevertLoss) {
				QSettings.Instance.EnableRevertLoss = _bool;
				RectSettings.height = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (QSettings.Instance.EnableRevertLoss) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Time to keep the revert function:", GUIStyleSettingsLabel, GUILayout.Width (250));
				TimeToKeep = GUILayout.TextField (TimeToKeep, GUIStyleSettingsTextField, GUILayout.Width (75));
				_int = 15;
				int.TryParse (TimeToKeep, out _int);
				QSettings.Instance.TimeToKeep = _int * 60;
				GUILayout.Label ("min(s)", GUIStyleSettingsLabel, GUILayout.Width (50));
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);
			}

			GUILayout.BeginHorizontal ();
			QSettings.Instance.StockToolBar = GUILayout.Toggle (QSettings.Instance.StockToolBar, "Use the Stock ToolBar", GUILayout.Width (250));
			if (QBlizzyToolbar.isAvailable) {
				QSettings.Instance.BlizzyToolBar = GUILayout.Toggle (QSettings.Instance.BlizzyToolBar, "Use the Blizzy ToolBar", GUILayout.Width (250));
			}
			GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box ("Finance", GUILayout.Height (30));
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);
				_bool = QCareer.useRevertCost;
				if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
					GUILayout.BeginHorizontal ();
					QSettings.Instance.Credits = GUILayout.Toggle (QSettings.Instance.Credits, "Revert will cost fund", GUILayout.Width (250));
					if (QSettings.Instance.Credits) {
						CreditsCost = GUILayout.TextField (CreditsCost);
						_int = 1000;
						int.TryParse (CreditsCost, out _int);
						QSettings.Instance.CreditsCost = _int;
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					GUILayout.BeginHorizontal ();
					QSettings.Instance.Reputations = GUILayout.Toggle (QSettings.Instance.Reputations, "Revert will cost reputation", GUILayout.Width (250));
					if (QSettings.Instance.Reputations) {
						ReputationsCost = GUILayout.TextField (ReputationsCost);
						_int = 10;
						int.TryParse (ReputationsCost, out _int);
						QSettings.Instance.ReputationsCost = _int;
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
				}
				GUILayout.BeginHorizontal ();
				QSettings.Instance.Sciences = GUILayout.Toggle (QSettings.Instance.Sciences, "Revert will cost science", GUILayout.Width (250));
				if (QSettings.Instance.Sciences) {
					SciencesCost = GUILayout.TextField (SciencesCost);
					_int = 5;
					int.TryParse (SciencesCost, out _int);
					QSettings.Instance.SciencesCost = _int;
				}
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);
				if (_bool != QCareer.useRevertCost) {
					RectSettings.height = 0;
				}
				if (QCareer.useRevertCost) {
					GUILayout.BeginHorizontal ();
					GUILayout.Space (5);
					GUILayout.Label ("The amount of costs is influenced by:", GUIStyleSettingsLabel, GUILayout.Width (400));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Space (40);
					QSettings.Instance.CostFctReputations = GUILayout.Toggle (QSettings.Instance.CostFctReputations, "the reputation", GUILayout.Width (220));
					QSettings.Instance.CostFctPenalties = GUILayout.Toggle (QSettings.Instance.CostFctPenalties, "the penalties game difficulty", GUILayout.Width (220));
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					GUILayout.BeginHorizontal ();
					GUILayout.Space (40);
					QSettings.Instance.CostFctVessel = GUILayout.Toggle (QSettings.Instance.CostFctVessel, "the price of the vessel", GUILayout.Width (220));
					if (QSettings.Instance.CostFctVessel) {
						GUILayout.Label ("Vessel base price:", GUIStyleSettingsLabel, GUILayout.Width (125));
						VesselBasePrice = GUILayout.TextField (VesselBasePrice, GUIStyleSettingsTextField);
						_float = 50000;
						float.TryParse (VesselBasePrice, out _float);
						QSettings.Instance.VesselBasePrice = _float;
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Price factor for a Revert to Launch:", GUIStyleSettingsLabel, GUILayout.Width (250));
					RevertToLaunchFactor = GUILayout.TextField (RevertToLaunchFactor, GUIStyleSettingsTextField, GUILayout.Width (40));
					_float = 75;
					float.TryParse (RevertToLaunchFactor, out _float);
					QSettings.Instance.RevertToLaunchFactor = _float / 100;
					GUILayout.Label ("%", GUIStyleSettingsLabel, GUILayout.Width (12));
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					GUILayout.BeginHorizontal ();
					//GUILayout.Label (string.Format ("Example for a vessel which cost <color=#B4D455><b>{3}</b> funds</color>, it will cost you, for a:{0}- revert to editor: {1}{0}- revert to launch: {2}", Environment.NewLine, _msgCostEditor, _msgCostLaunch, QCareer.VesselCost.ToString ("N0")), GUIStyleSettingsLabel, GUILayout.Width (490));
					GUILayout.Label ("Example for a vessel which cost ", GUILayout.Width (200));
					string _text = GUILayout.TextField(text.ToString(), GUIStyleSettingsTextField, GUILayout.Width (75));
					float.TryParse (_text, out text);
					GUILayout.Label (" funds, it will cost you, for a:", GUIStyleSettingsLabel, GUILayout.Width (215));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					string _msgCost = QCareer.msgCost (QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Funds, text), QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Reputation, text), QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Science, text), true);
					GUILayout.Label ("- revert to editor:" + _msgCost, GUIStyleSettingsLabel, GUILayout.Width (490));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					_msgCost = QCareer.msgCost (QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Funds, text), QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Reputation, text), QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Science, text), true);
					GUILayout.Label ("- revert to launch:" + _msgCost, GUIStyleSettingsLabel, GUILayout.Width (490));
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
				}
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Close and Save",GUILayout.Height(30))) {
				if (!string.IsNullOrEmpty (TimeToKeep)) {
					if (!int.TryParse (TimeToKeep, out _int)) {
						QSettings.Instance.TimeToKeep = 900;
					} else {
						QSettings.Instance.TimeToKeep = _int * 60;
					}
				}
				if (!string.IsNullOrEmpty (CreditsCost)) {
					if (!int.TryParse (CreditsCost, out QSettings.Instance.CreditsCost)) {
						QSettings.Instance.CreditsCost = 1000;
					}
				}
				if (!string.IsNullOrEmpty (ReputationsCost)) {
					if (!int.TryParse (ReputationsCost, out QSettings.Instance.ReputationsCost)) {
						QSettings.Instance.ReputationsCost = 10;
					}
				}
				if (!string.IsNullOrEmpty (SciencesCost)) {
					if (!int.TryParse (SciencesCost, out QSettings.Instance.SciencesCost)) {
						QSettings.Instance.SciencesCost = 5;
					}
				}
				if (!string.IsNullOrEmpty (VesselBasePrice)) {
					if (!float.TryParse (VesselBasePrice, out QSettings.Instance.VesselBasePrice)) {
						QSettings.Instance.VesselBasePrice = 50000;
					}
				}
				if (!string.IsNullOrEmpty (RevertToLaunchFactor)) {
					if (!float.TryParse (RevertToLaunchFactor, out _float)) {
						QSettings.Instance.RevertToLaunchFactor = 0.75f;
					} else {
						if (_float > 100) {
							_float = 100;
						}
						QSettings.Instance.RevertToLaunchFactor = _float / 100;
					}
				}
				Settings ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			GUILayout.BeginHorizontal();
			GUILayout.Space(5);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}
}