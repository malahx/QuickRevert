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
	public class QFlightData {

		internal static string FileFlightState = KSPUtil.ApplicationRootPath + "GameData/" + QuickRevert.MOD + "/PluginData/{0}-flightstate.txt";

		internal static string PathFlightState {
			get {
				if (HighLogic.LoadedSceneIsGame) {
					return string.Format (FileFlightState, HighLogic.SaveFolder);
				}
				return string.Empty;
			}
		}

		internal static bool ConfigNodeHasPostInitState(ConfigNode nodes) {
			return nodes.HasNode ("PostInitState");
		}

		internal static bool ConfigNodeHasPreLaunchState(ConfigNode nodes) {
			return nodes.HasNode ("PreLaunchState") &&
				nodes.HasValue ("newShipFlagURL") &&
				nodes.HasValue ("newShipToLoadPath") &&
				nodes.HasNode ("ShipConfig") &&
				nodes.HasValue ("ShipType");
		}

		internal static bool isHardSaved {
			get {
				if (!HighLogic.LoadedSceneIsGame) {
					return false;
				}
				return File.Exists (PathFlightState);
			}
		}

		internal static bool CanStorePostInitState {
			get {
				if (!HighLogic.LoadedSceneIsFlight) {
					return false;
				}
				return FlightDriver.PostInitState != null;
			}
		}

		internal bool CanReStorePostInitState {
			get {
				return PostInitState != null;
			}
		}

		internal static bool CanStorePreLaunchState {
			get {
				if (!HighLogic.LoadedSceneIsFlight) {
					return false;
				}
				return FlightDriver.PreLaunchState != null &&
					FlightDriver.newShipFlagURL != string.Empty &&
					FlightDriver.newShipToLoadPath != string.Empty &&
					ShipConstruction.ShipConfig != null &&
					ShipConstruction.ShipType != EditorFacility.None;
			}
		}

		internal bool CanReStorePreLaunchState {
			get {
				return PreLaunchState != null &&
					newShipFlagURL != string.Empty &&
					newShipToLoadPath != string.Empty &&
					ShipConfig != null &&
					ShipType != EditorFacility.None;
			}
		}

		public QFlightData() {
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
		public double time {
			get;
			private set;
		}

		public bool CanFindVessel {
			get {
				return HighLogic.LoadedSceneHasPlanetarium && FlightGlobals.Vessels.Count > 0;
			}
		}
		public Guid VesselGuid {
			get {
				if (PostInitState == null) {
					return Guid.Empty;
				}
				return PostInitState.ActiveVesselID;
			}
		}
		public int activeVesselIdx {
			get {
				if (PostInitState == null) {
					return -1;
				}
				return PostInitState.ActiveVessel;
			}
		}
		public bool VesselExists {
			get {
				Guid _guid = VesselGuid;
				if (_guid == Guid.Empty) {
					return false;
				}
				if (CanFindVessel) {
					return FlightGlobals.Vessels.Exists(v => v.id == _guid);
				}
				return HighLogic.CurrentGame.Updated().flightState.protoVessels.Exists(pv => pv.vesselID == _guid);
			}
		}
		public ProtoVessel pVessel {
			get {
				Guid _guid = VesselGuid;
				if (_guid == Guid.Empty) {
					return null;
				}
				if (CanFindVessel) {
					Vessel _vessel = FlightGlobals.Vessels.FindLast (v => v.id == _guid);
					if (_vessel != null) {
						return _vessel.protoVessel;
					}
				}
				return HighLogic.CurrentGame.Updated().flightState.protoVessels.FindLast (pv => pv.vesselID == _guid);
			}
		}
		public Vessel vessel {
			get {
				Guid _guid = VesselGuid;
				if (_guid == Guid.Empty || !CanFindVessel) {
					return null;
				}
				Vessel _vessel = FlightGlobals.Vessels.FindLast (v => v.id == _guid);
				return _vessel;
			}
		}
		public bool isPrelaunch {
			get {
				if (CanFindVessel) {
					Vessel _vessel = vessel;
					if (_vessel != null) {
						return _vessel.situation == Vessel.Situations.PRELAUNCH;
					}
				}
				ProtoVessel _pVessel = pVessel;
				if (_pVessel == null) {
					return false;
				}
				return _pVessel.situation == Vessel.Situations.PRELAUNCH;
			}
		}
		public bool isActiveVessel {
			get {
				Guid _guid = VesselGuid;
				if (_guid == Guid.Empty || FlightGlobals.ActiveVessel == null) {
					return false;
				}
				return _guid == FlightGlobals.ActiveVessel.id;
			}
		}
		public bool PostInitStateIsSaved {
			get {
				return PostInitState != null;
			}
		}
		public bool PreLaunchStateIsSaved {
			get {
				return PreLaunchState != null &&
					newShipToLoadPath != string.Empty &&
					newShipFlagURL != string.Empty &&
					ShipConfig != null &&
					ShipType != EditorFacility.None;
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
			time = Planetarium.GetUniversalTime ();
			QuickRevert.Log ("Store data");
		}
		public void Restore() {
			FlightDriver.PostInitState = PostInitState;
			#if COST
			FlightDriver.CanRevertToPostInit = QCareer.CanRevertToPostInit;
			#else
			FlightDriver.CanRevertToPostInit = true;
			#endif
			FlightDriver.PreLaunchState = PreLaunchState;
			#if COST
			FlightDriver.CanRevertToPrelaunch = QCareer.CanRevertToPrelaunch;
			#else
				FlightDriver.CanRevertToPrelaunch = true;
			#endif
			FlightDriver.newShipToLoadPath = newShipToLoadPath;
			FlightDriver.newShipFlagURL = newShipFlagURL;
			ShipConstruction.ShipConfig = ShipConfig;
			ShipConstruction.ShipType = ShipType;
			time = Planetarium.GetUniversalTime ();
			QuickRevert.Log ("Restore data");
		}
		public void Refresh() {
			bool _return = false;
			if (FlightDriver.PostInitState != null && PostInitState != null) {
				if (FlightDriver.PostInitState.UniversalTime == PostInitState.UniversalTime) {
					PostInitState = FlightDriver.PostInitState;
					QuickRevert.Warning ("PostInitState refreshed");
					_return = true;
				}
			}
			if (FlightDriver.PreLaunchState != null && PreLaunchState != null) {
				if (FlightDriver.PreLaunchState.UniversalTime == PreLaunchState.UniversalTime) {
					PreLaunchState = FlightDriver.PreLaunchState;
					QuickRevert.Warning ("PreLaunchState refreshed");
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
			time = 0;
			if (PathFlightState != string.Empty) {
				if (File.Exists (PathFlightState)) {
					File.Delete (PathFlightState);
				}
			}
			QuickRevert.Log ("Reset data");
		}
		public void Save() {
			if (PostInitStateIsSaved) {
				if (VesselExists) { 
					ConfigNode _flightstate = new ConfigNode ();
					_flightstate.AddNode ("PostInitState").AddData (PostInitState.Config);
					if (PreLaunchStateIsSaved) {
						_flightstate.AddNode ("PreLaunchState").AddData (PreLaunchState.Config);
						_flightstate.AddValue ("newShipFlagURL", newShipFlagURL);
						_flightstate.AddValue ("newShipToLoadPath", newShipToLoadPath);
						_flightstate.AddNode ("ShipConfig").AddData (ShipConfig);
						_flightstate.AddValue ("ShipType", (ShipType == EditorFacility.SPH ? "SPH" : "VAB"));
					}
					try {
						_flightstate.Save (PathFlightState);
						QuickRevert.Log ("Hard save data");
						return;
					} catch (Exception e) {
						QuickRevert.Warning (string.Format ("Can't hard save flight state: {0}", e));
					}
				}
			} else {
				QuickRevert.Warning ("Nothing to save.");
				Reset ();
			}
		}
		public void SaveTime() {
			if (PostInitStateIsSaved) {
				if (VesselExists) { 
					time = Planetarium.GetUniversalTime ();
					QuickRevert.Log("Save Time", true);
				}
			}
		}
		public void Load() {
			if (isHardSaved) {
				ConfigNode _flightstate = ConfigNode.Load (PathFlightState);
				if (ConfigNodeHasPostInitState (_flightstate)) {
					Game _game = new Game (_flightstate.GetNode ("PostInitState"));
					if (!_game.compatible) {
						QuickRevert.Warning ("Post Init State is not compatible.");
						Reset ();
						return;
					}
					PostInitState = new GameBackup (_game);
					if (ConfigNodeHasPreLaunchState (_flightstate)) {
						_game = new Game (_flightstate.GetNode ("PreLaunchState"));
						if (!_game.compatible) {
							QuickRevert.Warning ("Pre Launch State is not compatible.");
							Reset ();
							return;
						}
						PreLaunchState = new GameBackup (_game);
						newShipFlagURL = _flightstate.GetValue ("newShipFlagURL");
						newShipToLoadPath = _flightstate.GetValue ("newShipToLoadPath");
						ShipConfig = _flightstate.GetNode ("ShipConfig");
						ShipType = (_flightstate.GetValue ("ShipType") == "SPH" ? EditorFacility.SPH : EditorFacility.VAB);
					}
					time = 0;
					QuickRevert.Log ("Load Hard Saved Data");
					return;
				} else {
					QuickRevert.Warning ("Flight state is not correctly saved.");
					Reset ();
					return;
				}
			} else {
				QuickRevert.Warning ("Nothing to load.");
				Reset ();
				return;
			}
		}
	}
}