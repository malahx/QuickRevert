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
	public class QuickRevert : Quick {

		internal static QuickRevert Instance;
		#if GUI
		[KSPField(isPersistant = true)] internal static QBlizzyToolbar BlizzyToolbar;
		[KSPField(isPersistant = true)] internal static QStockToolbar StockToolbar;
		#endif

		private void Awake() {
			Instance = this;
			QSettings.Instance.Load ();
			#if GUI
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			if (StockToolbar == null) StockToolbar = new QStockToolbar ();
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
				GameEvents.onGUIApplicationLauncherDestroyed.Add (StockToolbar.AppLauncherDestroyed);
				GameEvents.onGameSceneLoadRequested.Add (StockToolbar.AppLauncherDestroyed);
				GameEvents.onGUIApplicationLauncherUnreadifying.Add (StockToolbar.AppLauncherDestroyed);
				QGUI.Awake ();
			}
			#endif
			if (HighLogic.LoadedSceneIsGame) {
				if (!HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					AutoDestroy ();
				}
				GameEvents.onFlightReady.Add (OnFlightReady);
				#if KEEP
				GameEvents.onTimeWarpRateChanged.Add (OnTimeWarpRateChanged);
				GameEvents.onGameStateLoad.Add (OnGameStateLoad);
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
		}

		private void AutoDestroy() {
			Quick.Warning ("Revert is disabled on this game.");
			Destroy (this);
		}

		private void Start() {
			if (HighLogic.LoadedSceneIsGame) {
				#if KEEP
				if (QFlight.CanTimeLostDataSaved) {
					StartEach ();
				}
				#endif
				#if GUI
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
					BlizzyToolbar.Start ();
					StartCoroutine (StockToolbar.AppLauncherReady ());
				}
				#endif
			}
			#if KEEP
			QFlight.Start ();
			#endif
		}

		private void OnDestroy() {
			#if GUI
			BlizzyToolbar.OnDestroy ();
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (StockToolbar.AppLauncherDestroyed);
			GameEvents.onGameSceneLoadRequested.Remove (StockToolbar.AppLauncherDestroyed);
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove (StockToolbar.AppLauncherDestroyed);
			#endif
			GameEvents.onFlightReady.Remove (OnFlightReady);
			#if KEEP
			GameEvents.onTimeWarpRateChanged.Remove (OnTimeWarpRateChanged);
			GameEvents.onGameStateLoad.Remove (OnGameStateLoad);
			StopEach ();
			#endif
			#if COST
			GameEvents.onGamePause.Remove (OnGamePause);
			#endif
		}

		private void OnFlightReady() {
			#if COST
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
				QCareer.OnFlightReady ();
			}
			#endif
			#if KEEP
			QFlight.OnFlightReady ();
			if (QFlight.CanTimeLostDataSaved) {
				StartEach ();
			}
			#endif
		}

		#if COST
		private void OnGamePause() {
			if (!HighLogic.LoadedSceneIsFlight || !QCareer.useRevertCost) {
				return;
			}
			StartCoroutine (OnRevertPopup ());
		}
		#endif

		#if GUI
		private void OnGUI() {
			QGUI.OnGUI ();
		}
		#endif

		#if KEEP
		private void OnTimeWarpRateChanged() {
			RestartEach ();
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
					if (QFlight.data.isSaved) {
						if (!QFlight.data.VesselExists && _UTNode == QFlight.data.PreLaunchState.UniversalTime) {
							Quick.Warning ("Revert to EDITOR.");
							QFlight.data.Reset ();
							return;
						}
					}
				}
			}
			if (HighLogic.LoadedSceneIsFlight) {
				if (FlightDriver.StartupBehaviour == FlightDriver.StartupBehaviours.RESUME_SAVED_CACHE) {
					if (QFlight.data.isSaved) {
						int _ActiveVesselIdx;
						if (int.TryParse (node.GetNode ("FLIGHTSTATE").GetValue ("activeVessel"), out _ActiveVesselIdx)) {
							if (_ActiveVesselIdx == QFlight.data.activeVesselIdx) {
								if (QFlight.data.isPrelaunch && Math.Round (_UTNode, 11) == Math.Round (QFlight.data.PostInitState.UniversalTime, 11) && Math.Round (Planetarium.GetUniversalTime (), 11) == Math.Round (QFlight.data.PostInitState.UniversalTime, 11)) {
									Quick.Warning ("Revert to LAUNCH.");
									QFlight.data.Reset ();
									return;
								} else if (_UTNode > QFlight.data.PostInitState.UniversalTime) {
									Quick.Warning ("Quickload.");
									return;
								} 
							}
						}
					}
				}	
			}
			if (!HighLogic.LoadedSceneIsEditor) {
				if (QFlight.data.isSaved && !QFlight.data.VesselExists) {
					if (_UTNode < QFlight.data.PostInitState.UniversalTime) {
						Quick.Warning ("Quickload an older quicksave");
						QFlight.data.Reset ();
					}
				}
			}
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
						Quick.Warning ("Revert to Launch (OnGameSceneLoadRequested)");
					}
				}
			} else {
				if (FlightDriver.PreLaunchState != null && EditorDriver.StartupBehaviour == EditorDriver.StartupBehaviours.LOAD_FROM_CACHE) {
					Game _game = GamePersistence.LoadGame (SaveGame, HighLogic.SaveFolder, true, true);
					if (FlightDriver.PreLaunchState.UniversalTime == _game.UniversalTime) {
						Quick.Warning ("Revert to Editor (OnGameSceneLoadRequested)");
					}
				}
			}
		}*/
	}
}