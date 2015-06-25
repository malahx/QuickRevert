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
	public class QFlight : MonoBehaviour {
		private static double last_scrMSG = 0;

		[KSPField(isPersistant = true)]	internal static QFlightData data;

		internal static bool isKeptDataSaved {
			get {
				Vessel _vessel = data.vessel;
				if (_vessel != null) {
					if (_vessel.loaded) {
						return true;
					}
					if (isPrelaunch (_vessel)) {
						return true;
					}
				}
				if (FlightGlobals.ActiveVessel != null) {
					if (FlightGlobals.ActiveVessel.id == data.VesselGuid) {
						return true;
					}
					if (FlightGlobals.ActiveVessel.isEVA) {
						return true;
					}
				}
				return false;
			}
		}

		internal static bool CanTimeLostDataSaved {
			get {
				if (!QSettings.Instance.EnableRevertLoss) {
					return false;
				}
				if (!HighLogic.LoadedSceneHasPlanetarium) {
					return false;
				}
				if (!data.PostInitStateIsSaved) {
					return false;
				}
				if (!data.VesselExists) {
					return false;
				}
				return true;
			}
		}

		internal static bool isPrelaunch(Vessel vessel) {
			if (vessel == null) {
				return false;
			}
			return (vessel.situation == Vessel.Situations.PRELAUNCH);
		}

		internal static bool isPrelaunch(ProtoVessel pVessel) {
			if (pVessel == null) {
				return false;
			}
			return (pVessel.situation == Vessel.Situations.PRELAUNCH);
		}

		internal static void Awake() {
			if (data != null) {
				return;
			}
			data = new QFlightData ();
			QuickRevert.Log ("Init Flight Data");
		}

		internal static void Start() {
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				data.Reset ();
			}
			if (HighLogic.LoadedSceneIsGame) {
				if (QFlightData.isHardSaved) {
					if (!data.PostInitStateIsSaved) {
						data.Load ();
					}
				}
			}
		}

		internal static void StoreOrRestore() {
			if (data.PostInitStateIsSaved && data.isActiveVessel) {
				Restore ();
			} else if (isPrelaunch (FlightGlobals.ActiveVessel)) {
				Store ();
			} else {
				FlightDriver.CanRevertToPostInit = false;
				FlightDriver.CanRevertToPrelaunch = false;
			}
		}

		private static void Store() {
			if (QFlightData.CanStorePostInitState) {
				data.Reset ();
				data.Store ();
				data.Save ();
				QuickRevert.Log ("Revert stored");
			} else {
				QuickRevert.Log ("Revert can't be store.");
			}
		}

		private static void Restore() {
			if (data.PostInitStateIsSaved && data.VesselExists && isKeptDataSaved) {
				data.Restore ();
				QuickRevert.Log ("Revert restored");
			} else {
				QuickRevert.Log ("Nothing to Restore.");
			}
		}

		internal static void LostRevert() {
			QuickRevert.Warning ("LostRevert", true);
			if (data.VesselExists) {
				QuickRevert.Log (string.Format("You have lost the possibility to revert: {0}", data.pVessel.vesselName));
				ScreenMessages.PostScreenMessage (string.Format("[{0}] You have lost the possibility to revert the last launch ({1}).", QuickRevert.MOD, data.pVessel.vesselName), 10, ScreenMessageStyle.UPPER_RIGHT);
			}
			data.Reset ();
		}

		internal static bool KeepData {
			get {
				if (!CanTimeLostDataSaved) {
					return false;
				}
				if (data.isPrelaunch) {
					return true;
				}
				if (data.time == 0) {
					data.SaveTime ();
					return true;
				}
				if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready) {
					if (isKeptDataSaved) {
						if ((Planetarium.GetUniversalTime () - data.time) > 60) {
							data.SaveTime ();
						}
						return true;
					}
				}
				if ((Planetarium.GetUniversalTime () - data.time) > QSettings.Instance.TimeToKeep) {
					QuickRevert.Warning ("data.time: " + data.time);
					QuickRevert.Warning ("Planetarium.GetUniversalTime (): " + Planetarium.GetUniversalTime ());
					QuickRevert.Warning ("(Planetarium.GetUniversalTime () - data.time): " + (Planetarium.GetUniversalTime () - data.time).ToString());
					LostRevert ();
					return false;
				}
				double _time = (QSettings.Instance.TimeToKeep + data.time - Planetarium.GetUniversalTime ());
				if (_time > 30 && (Planetarium.GetUniversalTime () - last_scrMSG) > 59) {
					ScreenMessages.PostScreenMessage (string.Format ("[{0}] You will lose the possibility to revert the last launch in {1}.", QuickRevert.MOD, QuickRevert.TimeUnits (_time)), 10, ScreenMessageStyle.UPPER_RIGHT);
					last_scrMSG = Planetarium.GetUniversalTime ();
				}
				return true;
			}
		}
	}
}