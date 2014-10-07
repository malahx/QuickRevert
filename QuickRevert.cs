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
using KSP;
using UnityEngine;

namespace QuickRevert {
	[KSPAddon(KSPAddon.Startup.MainMenu | KSPAddon.Startup.EditorAny | KSPAddon.Startup.TrackingStation | KSPAddon.Startup.Flight | KSPAddon.Startup.SpaceCentre, false)]
	public class QuickRevert : MonoBehaviour {

		// Initialiser les variables
		public const string VERSION = "1.10";

		private static bool isdebug = true;
		private static bool ready = false;
		public static string Path_settings = KSPUtil.ApplicationRootPath + "GameData/QuickRevert/PluginData/QuickRevert/";
		private static double Time_to_Keep = 900;
		private static double Time_to_Save = 60;
		private static double last_scrMSG = 0;

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
		}

		// Supprimer les évènements
		private void OnDestroy() {
			GameEvents.onFlightReady.Remove (OnFlightReady);
			GameEvents.onLevelWasLoaded.Remove (OnLevelWasLoaded);
		}

		private void Update() {
			if (HighLogic.LoadedSceneIsGame && ready) {
				if (HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					if (Save_Vessel_Guid != Guid.Empty) {
						Vessel _vessel;
						if (VesselExist (Save_Vessel_Guid, out _vessel)) {
							if (!isPrelaunch (_vessel)) {
								if (!HighLogic.LoadedSceneIsFlight) {
									if (Save_Time == 0) {
										Save_Time = Planetarium.GetUniversalTime ();
										Save_time ();
									} else {
										if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Keep) {
											disable_Revert ();
										}
									}
								} else {
									if (FlightGlobals.ready) {
										if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid || FlightGlobals.ActiveVessel.isEVA) {
											if ((Planetarium.GetUniversalTime () - Save_Time) > Time_to_Save && Save_Time != 0) {
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
		}

		// Afficher le temps restant avant de perdre la fonction revert
		private void OnGUI() {
			if (ready && (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)) {
				if (HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					if ((Planetarium.GetUniversalTime () - last_scrMSG) > 60) {
						if (Save_Time != 0 && Save_Vessel_Guid != Guid.Empty) {
							Vessel _vessel;
							if (VesselExist (Save_Vessel_Guid, out _vessel)) {
								if (HighLogic.LoadedSceneIsFlight) {
									if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid) {
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
			if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH) {
				disable_Revert ();
				try {
					Save_FlightStateCache = FlightDriver.FlightStateCache;
				} catch {
					Debug ("Can't save Save_FlightStateCache !");
					Reset ();
					return;
				}
				try {
					Save_PostInitState = FlightDriver.PostInitState;
				} catch {
					Debug ("Can't save Save_PostInitState !");
					Reset ();
					return;
				}
				try {
					Save_PreLaunchState = FlightDriver.PreLaunchState;
				} catch {
					Debug ("Can't save Save_PreLaunchState try an other solution !");
					try {
						Save_PreLaunchState = new GameBackup(Save_FlightStateCache);
					} catch {
						Debug ("Can't save Save_PreLaunchState !");
					}
				}
				try {
					Save_ShipConfig = ShipConstruction.ShipConfig;
				} catch {
					Debug ("Can't save Save_ShipConfig !");
					Reset ();
				}
				Save_Vessel_Guid = FlightGlobals.ActiveVessel.id;
				Save_Time = Planetarium.GetUniversalTime();
				Debug("Revert saved");
				Save();
			} else {
				if (Save_Vessel_Guid != Guid.Empty && HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid) {
						try {  
							FlightDriver.FlightStateCache = Save_FlightStateCache;
						} catch {
							Debug ("Can't load Save_FlightStateCache !");
							Reset ();
							return;
						}
						try { 
							FlightDriver.PostInitState = Save_PostInitState;
							FlightDriver.CanRevertToPostInit = true;
						} catch {
							FlightDriver.CanRevertToPostInit = false;
							Debug ("Can't load Save_PostInitState !");
							Reset ();
							return;
						}
						try { 
							FlightDriver.PreLaunchState = Save_PreLaunchState;
							FlightDriver.CanRevertToPrelaunch = true;
						} catch {
							FlightDriver.CanRevertToPrelaunch = false;
							Debug ("Can't load Save_PreLaunchState !");
						}
						try { 
							ShipConstruction.ShipConfig = Save_ShipConfig;
						} catch {
							FlightDriver.CanRevertToPrelaunch = false;
							Debug ("Can't load Save_ShipConfig !");
						}
						Save_Time = Planetarium.GetUniversalTime ();
						Save_time ();
						Debug ("Revert loaded");
					}
				}
			}
		}

		// Désactiver la fonction de revert
		public static void disable_Revert() {
			Vessel _vessel;
			if (VesselExist (Save_Vessel_Guid, out _vessel)) {
				Debug ("You have lost the possibility to revert: " + _vessel.name);
				if (HighLogic.CurrentGame.Parameters.Flight.CanRestart) {
					if (HighLogic.LoadedSceneIsFlight) {
						if (FlightGlobals.ActiveVessel.id == Save_Vessel_Guid) {
							if (isPrelaunch (_vessel)) {
								goto ONE;
							}
						}
					}
					ScreenMessages.PostScreenMessage ("[QuickRevert] You have lost the possibility to revert the last launch.", 10, ScreenMessageStyle.UPPER_LEFT);
				}
			}
			ONE:
			Reset ();
		}

		// Charger la sauvegarde en cas d'arrêt de KSP
		private void OnLevelWasLoaded(GameScenes gamescenes) {
			ready = true;
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
				if (System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-flight.txt")) {
					if (Save_Vessel_Guid == Guid.Empty) {
						Load ();
					}
				}
			}
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				Save_Vessel_Guid = Guid.Empty;
				Save_Time = 0;
			}
		}

		// Afficher les messages de debug
		private static void Debug(string _string) {
			if (isdebug) {
				print ("QuickRevert" + VERSION + ": " + _string);
			}
		}

		// Sauvegarder les paramètres
		public static void Save() {
			if (Save_Vessel_Guid != Guid.Empty) {
				Vessel _vessel;
				if (VesselExist (Save_Vessel_Guid, out _vessel)) { 
					Save_time();
					try {	
					GamePersistence.SaveGame (Save_FlightStateCache, "QuickRevert_FlightStateCache", HighLogic.SaveFolder, SaveMode.OVERWRITE);
					} catch {
						Debug ("Can't persistent save FlightStateCache !");
						goto TWO;
					}
					try {
						GamePersistence.SaveGame (Save_PostInitState, "QuickRevert_PostInitState", HighLogic.SaveFolder, SaveMode.OVERWRITE);
					} catch {
						Debug ("Can't persistent save PostInitState !");
						goto TWO;
					}
					try {
						GamePersistence.SaveGame (Save_PreLaunchState, "QuickRevert_PreLaunchState", HighLogic.SaveFolder, SaveMode.OVERWRITE);
					} catch {
						try {
							GamePersistence.SaveGame (new GameBackup(Save_FlightStateCache), "QuickRevert_PreLaunchState", HighLogic.SaveFolder, SaveMode.OVERWRITE);
						} catch {
							Debug ("Can't persistent save PreLaunchState !");
						}
					}
					try {
						Save_ShipConfig.Save (Path_settings + HighLogic.SaveFolder + "-Save_ShipConfig.txt");
					} catch {
						Debug ("Can't persistent save ShipConfig !");
					}
					Debug("Save");
					return;
				}
			}
			TWO:
			Reset ();
		}

		// Sauvegarder le temps
		private static void Save_time() {
			ConfigNode _temp = new ConfigNode();
			_temp.AddValue ("Save_Time", Save_Time);
			_temp.AddValue ("Save_Vessel_Guid", Save_Vessel_Guid);
			_temp.Save (Path_settings + HighLogic.SaveFolder + "-flight.txt");
			Debug ("Save UniversalTime");
		}

		// Chargement des paramètres
		public static void Load() {
			if (System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-flight.txt")) {
				ConfigNode _temp = ConfigNode.Load (Path_settings + HighLogic.SaveFolder + "-flight.txt");
				Save_Time = Convert.ToDouble(_temp.GetValue ("Save_Time"));
				Save_Vessel_Guid = new Guid(_temp.GetValue ("Save_Vessel_Guid"));
				Vessel _vessel;
				if (!VesselExist(Save_Vessel_Guid, out _vessel)) {
					Reset ();
					return;
				}
				try {
					Save_FlightStateCache = GamePersistence.LoadGame ("QuickRevert_FlightStateCache", HighLogic.SaveFolder, true, false);
					Save_PostInitState = new GameBackup(GamePersistence.LoadGame ("QuickRevert_PostInitState", HighLogic.SaveFolder, true, false));
					Save_PreLaunchState = new GameBackup(GamePersistence.LoadGame ("QuickRevert_PreLaunchState", HighLogic.SaveFolder, true, false));
					Save_ShipConfig = ConfigNode.Load (Path_settings + HighLogic.SaveFolder + "-Save_ShipConfig.txt");
				} catch {
					Debug ("Can't load QuickRevert !");
					Reset ();
				}
				Debug("Load");
			}
		}

		// Remettre à zéro les paramètres
		public static void Reset() {
			Save_Vessel_Guid = Guid.Empty;
			Save_Time = 0;
			Debug ("Reset");
			if (System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-flight.txt") || System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-Save_ShipConfig.txt")) {
				if (System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-flight.txt")) {
					System.IO.File.Delete (Path_settings + HighLogic.SaveFolder + "-flight.txt");
				}
				if (System.IO.File.Exists (Path_settings + HighLogic.SaveFolder + "-Save_ShipConfig.txt")) {
					System.IO.File.Delete (Path_settings + HighLogic.SaveFolder + "-Save_ShipConfig.txt");
				}
				Debug ("Deleted flight.txt & Save_ShipConfig.txt");
			}
		}
	}
}