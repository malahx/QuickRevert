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
using System.IO;
using UnityEngine;

namespace QuickRevert {
	public class QData {

		internal static string FileFlightState = KSPUtil.ApplicationRootPath + "GameData/" + Quick.MOD + "/PluginData/{0}-flightstate.txt";

		internal static string PathFlightState {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return string.Format (FileFlightState, HighLogic.SaveFolder);
				}
				return string.Empty;
			}
		}

		internal static bool ConfigNodeHasSaved(ConfigNode nodes) {
			if (HighLogic.LoadedSceneIsGame) {
				return nodes.HasNode ("PostInitState") &&
				nodes.HasNode ("PreLaunchState") &&
				nodes.HasValue ("newShipFlagURL") &&
				nodes.HasValue ("newShipToLoadPath") &&
				nodes.HasNode ("ShipConfig") &&
				nodes.HasValue ("ShipType");
			}
			return false;
		}

		internal static bool isHardSaved {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return File.Exists (PathFlightState);
				}
				return false;
			}
		}

		internal static bool CanBeStore {
			get {
				if (HighLogic.LoadedSceneIsFlight) {
					return FlightDriver.PostInitState != null &&
						FlightDriver.PreLaunchState != null &&
						FlightDriver.newShipFlagURL != string.Empty &&
						FlightDriver.newShipToLoadPath != string.Empty &&
						ShipConstruction.ShipConfig != null &&
						ShipConstruction.ShipType != EditorFacility.None;
				}
				return false;
			}
		}

		public QData() {
			Reset ();
		}
		public string newShipToLoadPath {
			get;
			private set;
		}
		public string newShipFlagURL {
			get;
			private set;
		}
		public GameBackup PostInitState {
			get;
			private set;
		}
		public GameBackup PreLaunchState {
			get;
			private set;
		}
		public ConfigNode ShipConfig {
			get;
			private set;
		}
		public EditorFacility ShipType {
			get;
			private set;
		}
		public ProtoVessel pVessel {
			get;
			private set;
		}
		public double time {
			get;
			private set;
		}
		public Guid VesselGuid {
			get {
				if (PostInitState != null) {
					if (pVessel != null) {
						if (PostInitState.ActiveVesselID == pVessel.vesselID) {
							return pVessel.vesselID;
						} else {
							return Guid.Empty; 
						}
					}
					return PostInitState.ActiveVesselID;
				}
				return Guid.Empty;
			}
		}
		public int activeVesselIdx {
			get {
				if (PostInitState != null) {
					return PostInitState.ActiveVessel;
				}
				return -1;
			}
		}
		public ProtoVessel FindpVessel {
			get {
				Guid _guid = VesselGuid;
				if (_guid != Guid.Empty) {
					return HighLogic.CurrentGame.flightState.protoVessels.FindLast (pv => pv.vesselID == _guid);
				}
				return null;
			}
		}
		public bool VesselExists {
			get {
				Guid _guid = VesselGuid;
				if (_guid != Guid.Empty) {
					return HighLogic.CurrentGame.flightState.protoVessels.Exists(pv => pv.vesselID == _guid);
				}
				return false;
			}
		}
		public bool isPrelaunch {
			get {
				if (pVessel != null) {
					Vessel _vessel = pVessel.vesselRef;
					if (_vessel != null) {
						return _vessel.situation == Vessel.Situations.PRELAUNCH;
					}
					return pVessel.situation == Vessel.Situations.PRELAUNCH;
				}
				return false;
			}
		}
		public bool isActiveVessel {
			get {
				Guid _guid = VesselGuid;
				if (_guid != Guid.Empty && FlightGlobals.ActiveVessel != null) {
					return _guid == FlightGlobals.ActiveVessel.id;
				}
				return false;
			}
		}
		public bool isSaved {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return PostInitState != null &&
					PreLaunchState != null &&
					newShipToLoadPath != string.Empty &&
					newShipFlagURL != string.Empty &&
					ShipConfig != null &&
					ShipType != EditorFacility.None &&
					time != 0;
				}
				return false;
			}
		}
		public void Store() {
			PostInitState = FlightDriver.PostInitState;
			PreLaunchState = FlightDriver.PreLaunchState;
			#if COST
			FlightDriver.CanRevertToPostInit = QCareer.CanRevertToPostInit;
			FlightDriver.CanRevertToPrelaunch = QCareer.CanRevertToPrelaunch;
			#else
			FlightDriver.CanRevertToPostInit = true;
			FlightDriver.CanRevertToPrelaunch = true;
			#endif
			newShipToLoadPath = FlightDriver.newShipToLoadPath;
			newShipFlagURL = FlightDriver.newShipFlagURL;
			ShipConfig = ShipConstruction.ShipConfig;
			ShipType = ShipConstruction.ShipType;
			pVessel = FindpVessel;
			time = Planetarium.GetUniversalTime ();
			Quick.Log ("Store data");
		}
		public void Restore() {
			FlightDriver.PostInitState = PostInitState;
			FlightDriver.PreLaunchState = PreLaunchState;
			#if COST
			FlightDriver.CanRevertToPostInit = QCareer.CanRevertToPostInit;
			FlightDriver.CanRevertToPrelaunch = QCareer.CanRevertToPrelaunch;
			#else
			FlightDriver.CanRevertToPostInit = true;
			FlightDriver.CanRevertToPrelaunch = true;
			#endif
			FlightDriver.newShipToLoadPath = newShipToLoadPath;
			FlightDriver.newShipFlagURL = newShipFlagURL;
			ShipConstruction.ShipConfig = ShipConfig;
			ShipConstruction.ShipType = ShipType;
			pVessel = FindpVessel;
			time = Planetarium.GetUniversalTime ();
			Quick.Log ("Restore data");
		}
		public void Refresh() {
			bool _return = false;
			if (FlightDriver.PostInitState != null && PostInitState != null) {
				if (FlightDriver.PostInitState.UniversalTime == PostInitState.UniversalTime) {
					PostInitState = FlightDriver.PostInitState;
					Quick.Warning ("PostInitState refreshed");
					_return = true;
				}
			}
			if (FlightDriver.PreLaunchState != null && PreLaunchState != null) {
				if (FlightDriver.PreLaunchState.UniversalTime == PreLaunchState.UniversalTime) {
					PreLaunchState = FlightDriver.PreLaunchState;
					Quick.Warning ("PreLaunchState refreshed");
					_return = true;
				}
			}
			if (_return) {
				Save ();
			}
		}
		public void Reset() {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.CanRevertToPostInit = false;
				FlightDriver.CanRevertToPrelaunch = false;
			}
			PostInitState = null;
			PreLaunchState = null;
			newShipToLoadPath = string.Empty;
			newShipFlagURL = string.Empty;
			ShipConfig = null;
			ShipType = EditorFacility.None;
			pVessel = null;
			time = 0;
			if (HighLogic.LoadedSceneIsGame) {
				if (File.Exists (PathFlightState)) {
					File.Delete (PathFlightState);
				}
			}
			Quick.Log ("Reset data");
		}
		public void Save() {
			if (isSaved) {
				if (VesselExists) { 
					ConfigNode _flightstate = new ConfigNode ();
					_flightstate.AddNode ("PreLaunchState").AddData (PreLaunchState.Config);
					_flightstate.AddNode ("PostInitState").AddData (PostInitState.Config);
					_flightstate.AddValue ("newShipFlagURL", newShipFlagURL);
					_flightstate.AddValue ("newShipToLoadPath", newShipToLoadPath);
					_flightstate.AddNode ("ShipConfig").AddData (ShipConfig);
					_flightstate.AddValue ("ShipType", (ShipType == EditorFacility.SPH ? "SPH" : "VAB"));
					try {
						_flightstate.Save (PathFlightState);
						Quick.Log ("Hard save data");
						return;
					} catch (Exception e) {
						Quick.Warning (string.Format ("Can't hard save flight state: {0}", e));
					}
				}
			} else {
				Quick.Warning ("Nothing to save.");
				Reset ();
			}
		}
		public void SaveTime() {
			if (isSaved) {
				if (VesselExists) { 
					time = Planetarium.GetUniversalTime ();
					//Quick.Log("Save Time");
				}
			}
		}
		public void Load() {
			if (isHardSaved) {
				ConfigNode _flightstate = ConfigNode.Load (PathFlightState);
				if (ConfigNodeHasSaved (_flightstate)) {
					Game _game = new Game (_flightstate.GetNode ("PostInitState"));
					if (!_game.compatible) {
						Quick.Warning ("Post Init State is not compatible.");
						Reset ();
						return;
					}
					PostInitState = new GameBackup (_game);
					_game = new Game (_flightstate.GetNode ("PreLaunchState"));
					if (!_game.compatible) {
						Quick.Warning ("Pre Launch State is not compatible.");
						Reset ();
						return;
					}
					PreLaunchState = new GameBackup (_game);
					newShipFlagURL = _flightstate.GetValue ("newShipFlagURL");
					newShipToLoadPath = _flightstate.GetValue ("newShipToLoadPath");
					ShipConfig = _flightstate.GetNode ("ShipConfig");
					ShipType = (_flightstate.GetValue ("ShipType") == "SPH" ? EditorFacility.SPH : EditorFacility.VAB);
					pVessel = FindpVessel;
					time = Planetarium.GetUniversalTime ();
					Quick.Log ("Load Hard Saved Data");
					return;
				} else {
					Quick.Warning ("Flight state is not correctly saved.");
					Reset ();
					return;
				}
			} else {
				Quick.Warning ("Nothing to load.");
				Reset ();
				return;
			}
		}
	}
}