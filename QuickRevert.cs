/* 
QuickRevert
Copyright 2014 Malah

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
using System.IO;
using UnityEngine;

namespace QuickRevert {
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class QuickRevert : UnityEngine.MonoBehaviour {

		// Initialiser les variables
		public static string VERSION = "1.11";
		public static string MOD = "QuickRevert";

		private static bool isdebug = true;
		private static bool ready = false;
		public static string Path_settings = KSPUtil.ApplicationRootPath + "GameData/QuickRevert/PluginData/QuickRevert/";
		public static string Filename_flightsave = "{0}-flightsave.txt";
		public static string Filename_flightstate = "{0}-flightstate.txt";
		private static double Time_to_Keep = 900;
		private static double Time_to_Save = 60;
		private static double last_scrMSG = 0;

		[KSPField(isPersistant = true)]
		public static string Save_newShipToLoadPath;
		[KSPField(isPersistant = true)]
		public static string Save_newShipFlagURL;
		[KSPField(isPersistant = true)]
		public static Game Save_FlightStateCache;
		[KSPField(isPersistant = true)]
		public static GameBackup Save_PostInitState;
		[KSPField(isPersistant = true)]
		public static GameBackup Save_PreLaunchState;
		[KSPField(isPersistant = true)]
		private static ConfigNode Save_ShipConfig;
		[KSPField(isPersistant = true)]
		private static Guid Save_Vessel_Guid = Guid.Empty;
		[KSPField(isPersistant = true)]
		private static double Save_Time = 0;

		public static string Path_flightsave {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return string.Format (Path_settings + Filename_flightsave, HighLogic.SaveFolder);
				}
				return string.Empty;
			}
		}
		public static string Path_flightstate {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return string.Format (Path_settings + Filename_flightstate, HighLogic.SaveFolder);
				}
				return string.Empty;
			}
		}

		public static bool isHardSaved {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return File.Exists (Path_flightsave) || File.Exists (string.Format (Path_flightstate));
				}
				return false;
			}
		}

		public static bool isSaved {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return Save_newShipToLoadPath != string.Empty && Save_newShipFlagURL != string.Empty && Save_FlightStateCache != null && Save_PostInitState != null && Save_PreLaunchState != null && Save_ShipConfig != null && Save_Vessel_Guid != Guid.Empty && Save_Time != 0;
				}
				return false;
			}
		}

		public static bool ConfigNodeHasSaved(ConfigNode nodes) {
			if (HighLogic.LoadedSceneIsGame) {
				return nodes.HasValue("Save_newShipFlagURL") && nodes.HasValue("Save_newShipToLoadPath") && nodes.HasNode("Save_FlightStateCache") && nodes.HasNode("Save_PostInitState") && nodes.HasNode("Save_PreLaunchState") && nodes.HasNode("Save_ShipConfig");
			}
			return false;
		}

		public static bool CanbeSaved {
			get {
				if (HighLogic.LoadedSceneIsFlight) {
					return FlightDriver.newShipFlagURL != string.Empty && FlightDriver.newShipToLoadPath != string.Empty && FlightDriver.FlightStateCache != null && FlightDriver.PostInitState != null && FlightDriver.PreLaunchState != null && ShipConstruction.ShipConfig != null;
				}
				return false;
			}
		}

		public static bool FlightStateisCompatible {
			get {
				return Save_FlightStateCache.compatible;
			}
		}

		public static bool VesselExist(Guid _guid, out Vessel vessel) {
			vessel = null;
			foreach (Vessel _vessel in FlightGlobals.Vessels.ToArray()) {
				if (_vessel.id == _guid) {
					vessel = _vessel;
					return true;
				}
			}
			return false;
		}

		public static bool isPrelaunch(Guid _guid) {
			if (_guid != Guid.Empty) {
				Vessel _vessel;
				if (VesselExist (_guid, out _vessel)) {
					return (_vessel.situation == Vessel.Situations.PRELAUNCH);
				}
			}
			return false;
		}

		public static bool isPrelaunch(Vessel _vessel) {
			if (_vessel != null) {
				return (_vessel.situation == Vessel.Situations.PRELAUNCH);
			}
			return false;
		}

		public static string format_time(double _time) {
			if (_time >= 60) {
				return Math.Round (_time / 60) + " min(s)";
			} else {
				return _time + " sec(s)";
			}
		}

		// Préparer les évènements
		private void Awake() {
			GameEvents.onFlightReady.Add (OnFlightReady);
			GameEvents.onLevelWasLoaded.Add (OnLevelWasLoaded);
			GameEvents.onGameSceneLoadRequested.Add (OnGameSceneLoadRequested);
		}

		// Charger ou supprimer la sauvegarde existante
		private void Start() {
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
				if (isHardSaved) {
					if (!isSaved) {
						Load ();
						return;
					}
				}
			}
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				Reset ();
			}
		}

		// Supprimer les évènements
		private void OnDestroy() {
			GameEvents.onFlightReady.Remove (OnFlightReady);
			GameEvents.onLevelWasLoaded.Remove (OnLevelWasLoaded);
			GameEvents.onGameSceneLoadRequested.Remove (OnGameSceneLoadRequested);
		}

		// Gestion du temps restant
		private void Update() {
			if (HighLogic.LoadedSceneIsGame && ready) {
				if (isSaved) {
					Vessel _vessel;
					if (VesselExist (Save_Vessel_Guid, out _vessel)) {
						if (!isPrelaunch (_vessel)) {
							if (!HighLogic.LoadedSceneIsFlight) {
								if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Keep) {
									disable_Revert ();
								}
							} else {
								if (FlightGlobals.ready) {
									if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid || FlightGlobals.ActiveVessel.isEVA || _vessel.loaded) {
										if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Save) {
											Save_Time = Planetarium.GetUniversalTime ();
											Save_time ();
										}
									} else {
										if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Keep) {
											disable_Revert ();
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Afficher le temps restant avant de perdre la fonction revert
		private void OnGUI() {
			if (ready && (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)) {
				if (HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					if ((Planetarium.GetUniversalTime () - last_scrMSG) > 60) {
						if (isSaved) {
							Vessel _vessel;
							if (VesselExist (Save_Vessel_Guid, out _vessel)) {
								if (HighLogic.LoadedSceneIsFlight) {
									if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid || FlightGlobals.ActiveVessel.isEVA) {
										return;
									}
								}
								double _time = (Time_to_Keep + Save_Time - Planetarium.GetUniversalTime ());
								if (_time > 30) {
									ScreenMessages.PostScreenMessage ("[QuickRevert] You will lose the possibility to revert the last launch in " + format_time (_time) + ".", 10, ScreenMessageStyle.UPPER_LEFT);
								}
							}
						}
						last_scrMSG = Planetarium.GetUniversalTime ();
					}
				}
			}
		}

		// Activer le revert
		private void OnFlightReady() {
			if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH && !isSaved && CanbeSaved) {
				disable_Revert ();
				Save_FlightStateCache = FlightDriver.FlightStateCache;
				Save_PostInitState = FlightDriver.PostInitState;
				if (FlightDriver.PreLaunchState != null) {
					Save_PreLaunchState = FlightDriver.PreLaunchState;
				} else {
					Save_PreLaunchState = new GameBackup(Save_FlightStateCache);
				}
				Save_ShipConfig = ShipConstruction.ShipConfig;
				Save_newShipFlagURL = FlightDriver.newShipFlagURL;
				Save_newShipToLoadPath = FlightDriver.newShipToLoadPath;
				Save_Vessel_Guid = FlightGlobals.ActiveVessel.id;
				Save_Time = Planetarium.GetUniversalTime();
				Log("Revert saved");
				Save();
			} else {
				if (isSaved && HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					Vessel _vessel;
					if (VesselExist (Save_Vessel_Guid, out _vessel)) {
						if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid || FlightGlobals.ActiveVessel.isEVA || _vessel.loaded) {
							FlightDriver.FlightStateCache = Save_FlightStateCache;
							FlightDriver.PostInitState = Save_PostInitState;
							FlightDriver.CanRevertToPostInit = true;
							FlightDriver.PreLaunchState = Save_PreLaunchState;
							FlightDriver.CanRevertToPrelaunch = true;
							FlightDriver.newShipToLoadPath = Save_newShipToLoadPath;
							FlightDriver.newShipFlagURL = Save_newShipFlagURL;
							ShipConstruction.ShipConfig = Save_ShipConfig;
							Save_Time = Planetarium.GetUniversalTime ();
							Log ("Revert loaded");
							Save_time ();
						}
					} else {
						Warning ("Vessel of the flightstate doesn't exists (OnFlightReady).");
						Reset ();
					}
				}
			}
		}

		// Désactiver la fonction de revert
		public static void disable_Revert() {
			Vessel _vessel;
			if (VesselExist (Save_Vessel_Guid, out _vessel)) {
				Log (string.Format("You have lost the possibility to revert: {0}", _vessel.name));
				if (HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					ScreenMessages.PostScreenMessage (string.Format("[QuickRevert] You have lost the possibility to revert the last launch ({0}).", _vessel.name), 10, ScreenMessageStyle.UPPER_LEFT);
					Reset ();
				}
			}
		}

		// Supprimer la sauvegarde si le vaisseau n'existe pas.
		private void OnLevelWasLoaded(GameScenes gamescenes) {
			ready = true;
			if (HighLogic.LoadedSceneIsGame) {
				if (isSaved) {
					Vessel _vessel;
					if (!VesselExist (Save_Vessel_Guid, out _vessel)) {
						Warning ("Vessel of the flightstate doesn't exists (OnLevelWasLoaded).");
						Reset ();
						return;
					}
				}
			}
		}

		// Supprimer la sauvegarde du revert après un revert to editor.
		private void OnGameSceneLoadRequested(GameScenes gamescenes) {
			if (gamescenes == GameScenes.EDITOR && HighLogic.LoadedSceneIsFlight) {
				if (isSaved) {
					Warning ("Revert to EDITOR.");
					Reset ();
				}
			}
		}

		// Afficher les messages de debug
		private static void Log(string _string) {
			if (isdebug) {
				Debug.Log (MOD + "(" + VERSION + "): " + _string);
			}
		}
		private static void Warning(string _string) {
			if (isdebug) {
				Debug.LogWarning (MOD + "(" + VERSION + "): " + _string);
			}
		}

		// Sauvegarder les paramètres
		public static void Save() {
			if (isSaved) {
				Vessel _vessel;
				if (VesselExist (Save_Vessel_Guid, out _vessel)) { 
					Save_time();
					ConfigNode _flightstate = new ConfigNode ();
					ConfigNode _flightstatecache = new ConfigNode ();
					Save_FlightStateCache.Save (_flightstatecache);
					_flightstate.AddNode ("Save_FlightStateCache").AddData (_flightstatecache);
					_flightstate.AddNode ("Save_PreLaunchState").AddData (Save_PreLaunchState.Config);
					_flightstate.AddNode ("Save_PostInitState").AddData (Save_PostInitState.Config);
					_flightstate.AddNode ("Save_ShipConfig").AddData (Save_ShipConfig);
					_flightstate.AddValue ("Save_newShipFlagURL", Save_newShipFlagURL);
					_flightstate.AddValue ("Save_newShipToLoadPath", Save_newShipToLoadPath);
					try {
						_flightstate.Save (Path_flightstate);
					} catch (Exception e) {
						Warning (string.Format ("Can't hard save flight state: {0}", e));
						goto Reset;
					}
					Log("Hard save");
					return;
				}
			}
			Reset:
			Reset ();
		}

		// Sauvegarder le temps
		private static void Save_time() {
			ConfigNode _temp = new ConfigNode();
			_temp.AddValue ("Save_Time", Save_Time);
			_temp.AddValue ("Save_Vessel_Guid", Save_Vessel_Guid);
			_temp.Save (Path_flightsave);
			Log ("Save UniversalTime and Active Vessel.");
		}

		// Chargement des paramètres
		public static void Load() {
			if (isHardSaved) {
				ConfigNode _flightsave = ConfigNode.Load (Path_flightsave);
				if (!_flightsave.HasValue("Save_Time") || !_flightsave.HasValue("Save_Vessel_Guid")) {
					Warning ("No time or no vessel saved.");
					goto Reset;
				}
				Save_Vessel_Guid = new Guid(_flightsave.GetValue ("Save_Vessel_Guid"));
				Save_Time = Convert.ToDouble(_flightsave.GetValue ("Save_Time"));
				if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Keep) {
					Warning ("Time limit, stop the loading.");
					goto Reset;
				}
				ConfigNode _flightstate = ConfigNode.Load (Path_flightstate);
				if (ConfigNodeHasSaved (_flightstate)) {
					Save_FlightStateCache = new Game(_flightstate.GetNode ("Save_FlightStateCache"));
					if (!FlightStateisCompatible) {
						Warning ("Flight State Cache is not compatible.");
						goto Reset;
					}
					Game _game = new Game (_flightstate.GetNode ("Save_PostInitState"));
					if (!_game.compatible) {
						Warning ("Post Init State is not compatible.");
						goto Reset;
					}
					Save_PostInitState = new GameBackup (_game);
					_game = new Game (_flightstate.GetNode ("Save_PreLaunchState"));
					if (!_game.compatible) {
						Warning ("Pre Launch State is not compatible.");
						goto Reset;
					}
					Save_PreLaunchState = new GameBackup (_game);
					Save_ShipConfig = _flightstate.GetNode ("Save_ShipConfig");
					Save_newShipFlagURL = _flightstate.GetValue ("Save_newShipFlagURL");
					Save_newShipToLoadPath = _flightstate.GetValue ("Save_newShipToLoadPath");
				} else {
					Warning ("Flight state is not correctly saved.");
					goto Reset;
				}
				Log("Load");
				return;
			}
			Reset:
			Reset ();
		}

		// Remettre à zéro les paramètres
		public static void Reset() {
			Save_Vessel_Guid = Guid.Empty;
			Save_Time = 0;
			Save_FlightStateCache = null;
			Save_PostInitState = null;
			Save_PreLaunchState = null;
			Save_ShipConfig = null;
			Save_newShipFlagURL = string.Empty;
			Save_newShipToLoadPath = string.Empty;
			if (isHardSaved && HighLogic.LoadedSceneIsGame) {
				if (File.Exists (Path_flightsave)) {
					File.Delete (Path_flightsave);
				}
				if (File.Exists (Path_flightstate)) {
					File.Delete (Path_flightstate);
				}
			}
			Log ("Reset");
		}
	}
}