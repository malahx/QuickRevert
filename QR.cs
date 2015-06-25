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
using System.Collections;
using UnityEngine;

namespace QuickRevert {

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public partial class QuickRevert : MonoBehaviour {

		internal static QuickRevert Instance;
		#if GUI
		[KSPField(isPersistant = true)] internal static QBlizzyToolbar BlizzyToolbar;
		#endif

		private void Awake() {
			if (Instance != null) {
				Warning ("There's already an Instance of QuickRevert");
				Destroy (this);
				return;
			}
			Instance = this;
			#if GUI
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			#endif
			if (HighLogic.LoadedSceneIsGame) {
				if (!HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					Warning ("Revert is disabled on this game.");
					#if GUI
					if (QStockToolbar.Instance != null && ApplicationLauncher.Ready) {
						if (QStockToolbar.Instance.appLauncherButton != null) {
							QStockToolbar.Instance.Destroy ();
						}
						Destroy (QStockToolbar.Instance);
					}
					#endif
					Destroy (this);
					return;
				} else {
					#if GUI
					if (QStockToolbar.Instance == null) {
						QStockToolbar.Instantiate(new QStockToolbar());
					}
					#endif
				}
				GameEvents.onFlightReady.Add (OnFlightReady);
				#if KEEP
				GameEvents.onTimeWarpRateChanged.Add (OnTimeWarpRateChanged);
				GameEvents.onGameStateLoad.Add (OnGameStateLoad);
				GameEvents.onVesselRecovered.Add (OnVesselRecovered);
				GameEvents.onVesselChange.Add (OnVesselChange);
				#endif
				#if COST
				if (HighLogic.LoadedSceneIsFlight && QCareer.useRevertCost) {
					GameEvents.onGamePause.Add (OnGamePause);
				}
				#endif
			}
			#if KEEP
			QFlight.Awake ();
			#endif
			Warning ("Awake", true);
		}

		private void Start() {
			if (HighLogic.LoadedSceneIsGame) {
				QSettings.Instance.Load ();
				#if KEEP
				if (QFlight.CanTimeLostDataSaved) {
					StartEach ();
				}
				#endif
				#if GUI
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
					BlizzyToolbar.Start ();
					QGUI.Start ();
				}
				#endif
			}
			#if KEEP
			QFlight.Start ();
			#endif
			Warning ("Start", true);
		}

		private void OnDestroy() {
			#if GUI
			BlizzyToolbar.OnDestroy ();
			#endif
			GameEvents.onFlightReady.Remove (OnFlightReady);
			#if KEEP
			GameEvents.onTimeWarpRateChanged.Remove (OnTimeWarpRateChanged);
			GameEvents.onGameStateLoad.Remove (OnGameStateLoad);
			GameEvents.onVesselRecovered.Remove (OnVesselRecovered);
			GameEvents.onVesselChange.Remove (OnVesselChange);
			StopEach ();
			#endif
			#if COST
			GameEvents.onGamePause.Remove (OnGamePause);
			#endif
			Warning ("OnDestroy", true);
		}

		private void OnFlightReady() {
			#if COST
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
				QCareer.OnFlightReady ();
			}
			#endif
			#if KEEP
			QFlight.StoreOrRestore ();
			#endif
			Warning ("OnFlightReady", true);
		}

		#if COST
		private void OnGamePause() {
			if (!HighLogic.LoadedSceneIsFlight || !QCareer.useRevertCost) {
				return;
			}
			StartCoroutine (QCareer.OnRevertPopup ());
			Warning ("OnGamePause", true);
		}
		#endif

		#if KEEP
		private void OnVesselRecovered(ProtoVessel pVessel) {
			if (QFlight.data == null) {
				return;
			}
			if (QFlight.data.pVessel != pVessel) {
				return;
			}
			QFlight.data.Reset ();
			#if GUI
			QGUI.RectSettings.height = 0;
			#endif		
			Warning ("OnVesselRecovered", true);
		}

		private void OnVesselChange(Vessel vessel) {
			if (!FlightGlobals.ready) {
				return;
			}
			QFlight.StoreOrRestore ();
			Warning ("OnVesselChange", true);
		}
		#endif

		#if GUI
		private void OnGUI() {
			QGUI.OnGUI ();
		}
		#endif

		#if KEEP
		private void OnTimeWarpRateChanged() {
			if (!QFlight.CanTimeLostDataSaved) {
				StopEach ();
				return;
			}
			RestartEach ();
			Warning ("OnTimeWarpRateChanged", true);
		}

		private void OnGameStateLoad(ConfigNode node) {
			if (!node.HasNode ("FLIGHTSTATE")) {
				return;
			}
			if (!node.GetNode ("FLIGHTSTATE").HasValue ("UT")) {
				return;
			}
			double _UTNode = double.Parse (node.GetNode ("FLIGHTSTATE").GetValue ("UT"));
			if (!HighLogic.LoadedSceneIsFlight) {
				if (EditorDriver.StartupBehaviour == EditorDriver.StartupBehaviours.LOAD_FROM_CACHE) {
					if (QFlight.data.PostInitStateIsSaved) {
						if (!QFlight.data.VesselExists && _UTNode == QFlight.data.PreLaunchState.UniversalTime) {
							Warning ("Revert to EDITOR.");
							QFlight.data.Reset ();
							#if COST
							QFlight.data.Reset ();
							#endif
							return;
						}
					}
				}
			}
			if (HighLogic.LoadedSceneIsFlight) {
				if (FlightDriver.StartupBehaviour == FlightDriver.StartupBehaviours.RESUME_SAVED_CACHE) {
					if (QFlight.data.PostInitStateIsSaved) {
						int _ActiveVesselIdx;
						if (int.TryParse (node.GetNode ("FLIGHTSTATE").GetValue ("activeVessel"), out _ActiveVesselIdx)) {
							if (_ActiveVesselIdx == QFlight.data.activeVesselIdx) {
								if (QFlight.data.isPrelaunch && Math.Round (_UTNode, 11) == Math.Round (QFlight.data.PostInitState.UniversalTime, 11) && Math.Round (Planetarium.GetUniversalTime (), 11) == Math.Round (QFlight.data.PostInitState.UniversalTime, 11)) {
									Warning ("Revert to LAUNCH.");
									#if COST
									QFlight.data.Reset ();
									#endif
									return;
								} else if (_UTNode > QFlight.data.PostInitState.UniversalTime) {
									Warning ("Quickload.");
									return;
								} 
							}
						}
					}
				}	
			}
			if (!HighLogic.LoadedSceneIsEditor) {
				if (QFlight.data.PostInitStateIsSaved && !QFlight.data.VesselExists) {
					if (_UTNode < QFlight.data.PostInitState.UniversalTime) {
						Warning ("Quickload an older quicksave");
						QFlight.data.Reset ();
					}
				}
			}
			Warning ("OnGameStateLoad", true);
		}
		#endif
		//An other (simpler) way to find a revert without QFlight.data but it needs to load the savegame
		/*private void OnGameSceneLoadRequested(GameScenes gameScene) {
			if (!HighLogic.LoadedSceneIsFlight) {
				return;
			}
			if (gameScene == GameScenes.FLIGHT) {
				if (FlightDriver.PostInitState != null && FlightDriver.StartupBehaviour == FlightDriver.StartupBehaviours.RESUME_SAVED_CACHE) {
					Game _game = GamePersistence.LoadGame ("persistent", HighLogic.SaveFolder, true, true);
					// Don't know why but without round it's can't be equal
					if (Math.Round (FlightDriver.PostInitState.UniversalTime, 11) == Math.Round (_game.UniversalTime, 11)) {
						Warning ("Revert to Launch (OnGameSceneLoadRequested)");
					}
				}
			} else {
				if (FlightDriver.PreLaunchState != null && EditorDriver.StartupBehaviour == EditorDriver.StartupBehaviours.LOAD_FROM_CACHE) {
					Game _game = GamePersistence.LoadGame (SaveGame, HighLogic.SaveFolder, true, true);
					if (FlightDriver.PreLaunchState.UniversalTime == _game.UniversalTime) {
						Warning ("Revert to Editor (OnGameSceneLoadRequested)");
					}
				}
			}
		}*/
	}
}