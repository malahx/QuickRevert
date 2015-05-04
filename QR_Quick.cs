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
using System.Reflection;
using UnityEngine;

namespace QuickRevert {

	public class Quick : MonoBehaviour {

		public readonly static string VERSION = Assembly.GetAssembly(typeof(QuickRevert)).GetName().Version.Major + "." + Assembly.GetAssembly(typeof(QuickRevert)).GetName().Version.Minor + Assembly.GetAssembly(typeof(QuickRevert)).GetName().Version.Build;
		public readonly static string MOD = Assembly.GetAssembly(typeof(QuickRevert)).GetName().Name;
		private static bool isdebug = true;

		internal static void Log(string _string) {
			if (isdebug) {
				Debug.Log (MOD + "(" + VERSION + "): " + _string);
			}
		}
		internal static void Warning(string _string) {
			if (isdebug) {
				Debug.LogWarning (MOD + "(" + VERSION + "): " + _string);
			}
		}

		#if KEEP
		internal static string TimeUnits(double time) {
			if (time >= 60) {
				return Math.Round (time / 60) + " min(s)";
			} else {
				return time + " sec(s)";
			}
		}

		private Coroutine CoroutineEach;

		internal void StartEach() {
			if (CoroutineEach == null) {
				CoroutineEach = StartCoroutine (UpdateEach ());
			}
		}

		internal void StopEach() {
			if (CoroutineEach != null) {
				StopCoroutine (UpdateEach ());
				CoroutineEach = null;
			}
		}

		internal void RestartEach() {
			if (CoroutineEach != null) {
				StopEach ();
				StartEach ();
			}
		}

		internal IEnumerator UpdateEach () {
			yield return new WaitForSeconds (1);
			Coroutine _coroutine = CoroutineEach;
			//Quick.Log ("StartUpdateEach " + _coroutine.GetHashCode());
			while (_coroutine == CoroutineEach) {
				//Quick.Log ("UpdateEach " + _coroutine.GetHashCode());
				if (!QFlight.KeepData) {
					break;
				}
				yield return new WaitForSeconds ((TimeWarp.CurrentRateIndex == 0 ? 60 : 1));
			}
			//Quick.Log ("EndUpdateEach " + _coroutine.GetHashCode());
		}
		#endif

		#if COST
		internal IEnumerator OnRevertPopup () {
			while (FlightDriver.Pause) {
				PopupDialog _popup = (PopupDialog)PopupDialog.FindObjectOfType (typeof(PopupDialog));
				if (_popup != null) {
					if (_popup.dialogToDisplay != null) {
						if (_popup.enabled && _popup.dialogToDisplay.title == "Reverting Flight") {
							QCareer.OnRevertPopup (_popup);
						}
					}
				}
				yield return new WaitForEndOfFrame ();
			}
		}
		#endif
	}
}