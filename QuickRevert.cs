using System;
using KSP;
using UnityEngine;

namespace QuickRevert {
	[KSPAddon(KSPAddon.Startup.EditorAny | KSPAddon.Startup.TrackingStation | KSPAddon.Startup.Flight | KSPAddon.Startup.SpaceCentre, false)]
	public class QuickRevert : MonoBehaviour {

		// Initialiser les variables
		public const string VERSION = "1.00";

		public static bool enable = true;

		[KSPField(isPersistant = true)]
		private static GameBackup Save_PostInitState;
		[KSPField(isPersistant = true)]
		private static GameBackup Save_PreLaunchState;
		[KSPField(isPersistant = true)]
		private static Game Save_FlightStateCache;
		[KSPField(isPersistant = true)]
		private static ConfigNode Save_ShipConfig;

		// Préparer les évènements
		private void Awake() {
			GameEvents.onFlightReady.Add (OnFlightReady);
			GameEvents.onLevelWasLoaded.Add (OnLevelWasLoaded);
		}

		// Activer le revert après un quickload
		private void OnFlightReady() {
			if (enable) {
				if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH) {
					try {
						Save_FlightStateCache = FlightDriver.FlightStateCache;
						Save_PostInitState = FlightDriver.PostInitState;
						Save_PreLaunchState = FlightDriver.PreLaunchState;
						Save_ShipConfig = ShipConstruction.ShipConfig;
						print ("QuickRevert" + VERSION + ": Revert saved");
					} catch {
						print ("QuickRevert" + VERSION + ": Can't save !");
					}
				} else {
					try {  
						FlightDriver.FlightStateCache = Save_FlightStateCache;
						FlightDriver.PostInitState = Save_PostInitState;
						FlightDriver.PreLaunchState = Save_PreLaunchState;
						ShipConstruction.ShipConfig = Save_ShipConfig;
						FlightDriver.CanRevertToPostInit = true;
						FlightDriver.CanRevertToPrelaunch = true;
						print ("QuickRevert" + VERSION + ": Revert loaded");
					} catch {
						print ("QuickRevert" + VERSION + ": Can't load !");
					}					
				}
			}
		}

		// Initialiser les variables
		private void OnLevelWasLoaded(GameScenes gamescenes) {
			if (enable) {
				if (gamescenes != GameScenes.FLIGHT) {
					Save_FlightStateCache = null;
					Save_PostInitState = null;
					Save_PreLaunchState = null;
					Save_ShipConfig = null;
				}
			}
		}

		// Supprimer les évènements
		private void OnDestroy() {
			GameEvents.onFlightReady.Remove (OnFlightReady);
			GameEvents.onLevelWasLoaded.Remove (OnLevelWasLoaded);
		}
	}
}