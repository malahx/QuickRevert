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
	public class QSettings : MonoBehaviour {

		public readonly static QSettings Instance = new QSettings();

		internal static string FileConfig = KSPUtil.ApplicationRootPath + "GameData/" + Quick.MOD + "/Config.txt";

		#if GUI
		[Persistent]
		public bool StockToolBar = true;
		[Persistent]
		public bool BlizzyToolBar = true;
		#endif

		[Persistent]
		public bool EnableRevertLoss = true;

		#if KEEP
		[Persistent]
		public int TimeToKeep = 900;
		#endif

		#if COST
		[Persistent]
		public bool RevertCost = true;
		[Persistent]
		public bool Credits = true;
		[Persistent]
		public bool Sciences = false;
		[Persistent]
		public bool Reputations = false;
		[Persistent]
		public int CreditsCost = 1000;
		[Persistent]
		public int ReputationsCost = 10;
		[Persistent]
		public int SciencesCost = 5;
		[Persistent]
		public float RevertToLaunchFactor = 0.75f;
		[Persistent]
		public bool CostFctReputations = true;
		[Persistent]
		public bool CostFctVessel = true;
		[Persistent]
		public bool CostFctPenalties = true;
		[Persistent]
		public float VesselBasePrice = 50000;
		#endif

		public void Save() {
			ConfigNode _temp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
			_temp.Save(FileConfig);
			Quick.Log ("Settings Saved");
		}

		public void Load() {
			if (File.Exists (FileConfig)) {
				try {
					ConfigNode _temp = ConfigNode.Load (FileConfig);
					ConfigNode.LoadObjectFromConfig (this, _temp);
					Quick.Log ("Settings Loaded");
				} catch {
					Save ();
				}
			} else {
				Save ();
			}
		}
	}
}