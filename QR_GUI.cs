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
		internal static Rect RectSettings = new Rect();
		private static GUIStyle GUIStyleSettingsLabel;
		private static GUIStyle GUIStyleSettingsTextField;
		private static GUIStyle GUIStyleSettingsButton;

		private static float text;
		private static string MinPriceFactor;
		private static string MaxPriceFactor;
		private static string TimeToKeep;
		private static string CreditsCost;
		private static string ReputationsCost;
		private static string SciencesCost;
		private static string VesselBasePrice;
		private static string RevertToLaunchFactor;

		internal static void Start() {
			if (!HighLogic.LoadedSceneIsGame) {
				return;
			}
			text = 25000;
			MinPriceFactor = (QSettings.Instance.MinPriceFactor * 100).ToString();
			MaxPriceFactor = (QSettings.Instance.MaxPriceFactor * 100).ToString();
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
			GUIStyleSettingsLabel.contentOffset = new Vector2 ();
			GUIStyleSettingsLabel.margin = new RectOffset (GUIStyleSettingsLabel.margin.left,GUIStyleSettingsLabel.margin.right, 0 , 0);
			GUIStyleSettingsLabel.padding = new RectOffset ();

			GUIStyleSettingsTextField = new GUIStyle (HighLogic.Skin.textField);
			GUIStyleSettingsTextField.alignment = TextAnchor.UpperCenter;
			GUIStyleSettingsTextField.contentOffset = new Vector2 ();
			GUIStyleSettingsTextField.margin = new RectOffset (GUIStyleSettingsTextField.margin.left,GUIStyleSettingsTextField.margin.right, 0 , 0);
			GUIStyleSettingsTextField.padding = new RectOffset ();

			GUIStyleSettingsButton = new GUIStyle (HighLogic.Skin.button);
			GUIStyleSettingsButton.margin = new RectOffset (GUIStyleSettingsButton.margin.left,GUIStyleSettingsButton.margin.right, 0 , 0);
			GUIStyleSettingsButton.padding = new RectOffset ();

			RectSettings = new Rect ((Screen.width - 515) / 2, 40, 515, 0);
		}

		private static void Lock(bool activate, ControlTypes Ctrl) {
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
		}

		public static void Settings() {
			SettingsSwitch ();
			if (!WindowSettings) {
				QStockToolbar.Instance.Reset();
				QuickRevert.BlizzyToolbar.Reset();
				QSettings.Instance.Save ();
			}
		}

		internal static void SettingsSwitch() {
			WindowSettings = !WindowSettings;
			QStockToolbar.Instance.Set (WindowSettings);
			Lock (WindowSettings, ControlTypes.KSC_ALL);
		}

		internal static void OnGUI() {
			if (WindowSettings) {
				GUI.skin = HighLogic.Skin;
				RectSettings = GUILayout.Window (1584652, RectSettings, DrawSettings, QuickRevert.MOD + " " + QuickRevert.VERSION, GUILayout.Width (RectSettings.width), GUILayout.ExpandHeight(true));
			}
		}

		private static void DrawSettings(int id) {
			int _int;
			float _float;
			bool _bool;
			GUILayout.BeginVertical();

			if (QFlight.data.PostInitStateIsSaved) {
				if (QFlight.data.VesselExists) {
					GUILayout.BeginHorizontal();
					GUILayout.Box("Revert Saved",GUILayout.Height(30));
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					GUILayout.Label (string.Format("Revert of the last Vessel saved: <color=#FFFFFF><b>({0}){1}</b></color>", QFlight.data.pVessel.vesselType, QFlight.data.pVessel.vesselName), GUIStyleSettingsLabel, GUILayout.Width (400));
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Lose it", GUILayout.Width (50), GUILayout.Height (17))) {
						QFlight.data.Reset ();
						RectSettings.height = 0;
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
					GUILayout.BeginHorizontal ();
					if (QFlight.CanTimeLostDataSaved) {
						if (QFlight.data.isPrelaunch) {
							GUILayout.Label ("This vessel is in Prelaunch, you can't lose it.", GUIStyleSettingsLabel, GUILayout.Width (500));
						} else if (QFlight.data.time > 0) {
							GUILayout.Label ("The revert of this vessel will be lost in: " + QuickRevert.TimeUnits (QSettings.Instance.TimeToKeep - (Planetarium.GetUniversalTime () - QFlight.data.time)), GUIStyleSettingsLabel, GUILayout.Width (500));
						}
					}
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Goto it", GUILayout.Width (50), GUILayout.Height (17))) {
						Settings ();
						string _saveGame = GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
						FlightDriver.StartAndFocusVessel (_saveGame, QFlight.data.activeVesselIdx);
						InputLockManager.ClearControlLocks ();
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					if (!QFlight.data.PreLaunchStateIsSaved) {
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("<color=#FF0000><b>Only the revert to launch is saved!</b></color>", GUIStyleSettingsLabel, GUILayout.Width (500));
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
			_bool = GUILayout.Toggle (QSettings.Instance.EnableRevertLoss, "Enable the revert loss", GUILayout.Width (250));
			if (_bool != QSettings.Instance.EnableRevertLoss) {
				QSettings.Instance.EnableRevertLoss = _bool;
				if (QSettings.Instance.EnableRevertLoss) {
					QuickRevert.Instance.RestartEach ();
				} else {
					QuickRevert.Instance.StopEach ();
				}
				RectSettings.height = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);

			if (QSettings.Instance.EnableRevertLoss) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Time to keep the revert function:", GUIStyleSettingsLabel, GUILayout.Width (250));
				TimeToKeep = GUILayout.TextField (TimeToKeep, GUIStyleSettingsTextField, GUILayout.Width (75), GUILayout.Height(15));
				_int = 15;
				if (int.TryParse (TimeToKeep, out _int)) {
					QSettings.Instance.TimeToKeep = _int * 60;
				}
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
						CreditsCost = GUILayout.TextField (CreditsCost, GUIStyleSettingsTextField, GUILayout.Height(15));
						_int = 1000;
						if (int.TryParse (CreditsCost, out _int)) {
							QSettings.Instance.CreditsCost = _int;
						}
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
					GUILayout.BeginHorizontal ();
					QSettings.Instance.Reputations = GUILayout.Toggle (QSettings.Instance.Reputations, "Revert will cost reputation", GUILayout.Width (250));
					if (QSettings.Instance.Reputations) {
						ReputationsCost = GUILayout.TextField (ReputationsCost, GUIStyleSettingsTextField, GUILayout.Height(15));
						_int = 5;
						if (int.TryParse (ReputationsCost, out _int)) {
							QSettings.Instance.ReputationsCost = _int;
						}
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);
				}
				GUILayout.BeginHorizontal ();
				QSettings.Instance.Sciences = GUILayout.Toggle (QSettings.Instance.Sciences, "Revert will cost science", GUILayout.Width (250));
				if (QSettings.Instance.Sciences) {
					SciencesCost = GUILayout.TextField (SciencesCost, GUIStyleSettingsTextField, GUILayout.Height(15));
					_int = 1;
					if (int.TryParse (SciencesCost, out _int)) {
						QSettings.Instance.SciencesCost = _int;
					}
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
						VesselBasePrice = GUILayout.TextField (VesselBasePrice, GUIStyleSettingsTextField, GUILayout.Height(15));
						_float = 50000;
						if (float.TryParse (VesselBasePrice, out _float)) {
							QSettings.Instance.VesselBasePrice = _float;
						}
					}
					GUILayout.EndHorizontal ();
					GUILayout.Space (5);

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Price factor for a Revert to Launch:", GUIStyleSettingsLabel, GUILayout.Width (250));
					RevertToLaunchFactor = GUILayout.TextField (RevertToLaunchFactor, GUIStyleSettingsTextField, GUILayout.Width (40), GUILayout.Height(15));
					_float = 0.75f;
					if (float.TryParse (RevertToLaunchFactor, out _float)) {
						QSettings.Instance.RevertToLaunchFactor = _float / 100;
					}
					GUILayout.Label ("%", GUIStyleSettingsLabel, GUILayout.Width (12));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("The price can't be less than:", GUIStyleSettingsLabel, GUILayout.Width (250));
					MinPriceFactor = GUILayout.TextField (MinPriceFactor, GUIStyleSettingsTextField, GUILayout.Width (40), GUILayout.Height(15));
					_float = 0.5f;
					if (float.TryParse (MinPriceFactor, out _float)) {
						QSettings.Instance.MinPriceFactor = _float / 100;
					}
					GUILayout.Label ("% of the default price", GUIStyleSettingsLabel, GUILayout.Width (200));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("The price can't be higher than:", GUIStyleSettingsLabel, GUILayout.Width (250));
					MaxPriceFactor = GUILayout.TextField (MaxPriceFactor, GUIStyleSettingsTextField, GUILayout.Width (40), GUILayout.Height(15));
					_float = 2;
					if (float.TryParse (MaxPriceFactor, out _float)) {
						QSettings.Instance.MaxPriceFactor = _float / 100;
					}
					GUILayout.Label ("% of the default price", GUIStyleSettingsLabel, GUILayout.Width (200));
					GUILayout.EndHorizontal ();
					GUILayout.Space (15);

					GUILayout.BeginHorizontal ();
					//GUILayout.Label (string.Format ("Example for a vessel which cost <color=#B4D455><b>{3}</b> funds</color>, it will cost you, for a:{0}- revert to editor: {1}{0}- revert to launch: {2}", Environment.NewLine, _msgCostEditor, _msgCostLaunch, QCareer.VesselCost.ToString ("N0")), GUIStyleSettingsLabel, GUILayout.Width (490));
					GUILayout.Label ("Example for a vessel which cost ", GUIStyleSettingsLabel, GUILayout.Width (200));
					string _text = GUILayout.TextField (text.ToString (), GUIStyleSettingsTextField, GUILayout.Width (75), GUILayout.Height(15));
					_int = 25000;
					if (int.TryParse (_text, out _int)) {
						text = _int;
					}
					GUILayout.Label (" funds, it will cost you, for a:", GUIStyleSettingsLabel, GUILayout.Width (215));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Space (40);
					string _msgCost = QCareer.msgCost (QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Funds, text), QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Reputation, text), QCareer.Cost (QCareer.RevertType.EDITOR, Currency.Science, text), true);
					GUILayout.Label ("- revert to editor:" + _msgCost, GUIStyleSettingsLabel, GUILayout.Width (450));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Space (40);
					_msgCost = QCareer.msgCost (QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Funds, text), QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Reputation, text), QCareer.Cost (QCareer.RevertType.LAUNCH, Currency.Science, text), true);
					GUILayout.Label ("- revert to launch:" + _msgCost, GUIStyleSettingsLabel, GUILayout.Width (450));
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
				if (!string.IsNullOrEmpty (MinPriceFactor)) {
					if (!float.TryParse (MinPriceFactor, out _float)) {
						QSettings.Instance.MinPriceFactor = 0.5f;
					} else {
						QSettings.Instance.MinPriceFactor = _float / 100;
					}
				}
				if (!string.IsNullOrEmpty (MaxPriceFactor)) {
					if (!float.TryParse (MaxPriceFactor, out _float)) {
						QSettings.Instance.MaxPriceFactor = 2;
					} else {
						QSettings.Instance.MaxPriceFactor = _float / 100;
					}
				}
				if (QSettings.Instance.MinPriceFactor > QSettings.Instance.MaxPriceFactor) {
					QSettings.Instance.MinPriceFactor = QSettings.Instance.MaxPriceFactor;
				}
				if (QSettings.Instance.MinPriceFactor > 1) {
					QSettings.Instance.MinPriceFactor = 1;
				}
				if (QSettings.Instance.MaxPriceFactor < 1) {
					QSettings.Instance.MaxPriceFactor = 1;
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