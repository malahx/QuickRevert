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

		[KSPField(isPersistant = true)]
		internal static QData data;

		internal static bool isKeptDataSaved {
			get {
				bool _return = false;
				ProtoVessel _pVessel = data.pVessel;
				if (_pVessel != null) {
					Vessel _vessel = _pVessel.vesselRef;
					if (_vessel != null) {
						_return |= _vessel.loaded;
					}
				}
				if (FlightGlobals.ActiveVessel != null) {
					if (FlightGlobals.ActiveVessel.protoVessel != null) {
						_return |= FlightGlobals.ActiveVessel.protoVessel.vesselID == data.VesselGuid;
					}
					_return |= FlightGlobals.ActiveVessel.isEVA;
				}
				return _return;
			}
		}

		internal static bool CanTimeLostDataSaved {
			get {
				if (!QSettings.Instance.EnableRevertLoss) {
					return false;
				}
				if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.SPACECENTER && HighLogic.LoadedScene != GameScenes.TRACKSTATION) {
					return false;
				}
				if (!data.isSaved) {
					return false;
				}
				if (!data.VesselExists) {
					return false;
				}
				return true;
			}
		}

		internal static bool isPrelaunch(Vessel vessel) {
			if (vessel != null)
				return (vessel.situation == Vessel.Situations.PRELAUNCH);
			return false;
		}

		internal static void Awake() {
			if (data == null) {
				data = new QData ();
				Quick.Log ("Init Flight Data");
			}
		}

		internal static void Start() {
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				data.Reset ();
			}
			if (HighLogic.LoadedSceneIsGame) {
				if (QData.isHardSaved) {
					if (!data.isSaved) {
						data.Load ();
					}
				}
			}
		}

		internal static void OnFlightReady() {
			if (data.isSaved && data.isActiveVessel) {
				Restore ();
			} else if (isPrelaunch (FlightGlobals.ActiveVessel)) {
				Store ();
			}
		}

		private static void Store() {
			if (QData.CanBeStore) {
				data.Reset ();
				data.Store ();
				data.Save ();
				Quick.Log ("Revert stored");
			} else {
				Quick.Log ("Revert can't be store.");
			}
		}

		private static void Restore() {
			if (data.isSaved && data.VesselExists && isKeptDataSaved) {
				data.Restore ();
				Quick.Log ("Revert restored");
			} else {
				Quick.Log ("Nothing to Restore.");
			}
		}

		internal static void LostRevert() {
			if (data.VesselExists) {
				Quick.Log (string.Format("You have lost the possibility to revert: {0}", data.pVessel.vesselName));
				ScreenMessages.PostScreenMessage (string.Format("[{0}] You have lost the possibility to revert the last launch ({1}).", Quick.MOD, data.pVessel.vesselName), 10, ScreenMessageStyle.UPPER_RIGHT);
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
				if (HighLogic.LoadedSceneIsFlight) {
					if (FlightGlobals.ready) {
						if (isKeptDataSaved) {
							if ((Planetarium.GetUniversalTime () - data.time) > 60) {
								data.SaveTime ();
							}
							return true;
						} else {
							if ((Planetarium.GetUniversalTime () - data.time) > QSettings.Instance.TimeToKeep) {
								LostRevert ();
								return false;
							}
						}
					}
				} else if ((Planetarium.GetUniversalTime () - data.time) > QSettings.Instance.TimeToKeep) {
					LostRevert ();
					return false;
				}
				double _time = (QSettings.Instance.TimeToKeep + data.time - Planetarium.GetUniversalTime ());
				if (_time > 30 && (Planetarium.GetUniversalTime () - last_scrMSG) > 59) {
					ScreenMessages.PostScreenMessage (string.Format ("[{0}] You will lose the possibility to revert the last launch in {1}.", Quick.MOD, Quick.TimeUnits (_time)), 10, ScreenMessageStyle.UPPER_RIGHT);
					last_scrMSG = Planetarium.GetUniversalTime ();
				}
				return true;
			}
		}
	}
}