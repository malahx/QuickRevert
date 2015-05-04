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
using System.Collections.Generic;
using UnityEngine;

namespace QuickRevert {
	public class QCareer : MonoBehaviour {

		internal static bool CanRevertToPostInit = true;
		internal static bool CanRevertToPrelaunch = true;

		internal enum RevertType {
			EDITOR,
			LAUNCH
		}

		internal static bool useCredits {
			get {
				return QSettings.Instance.Credits && HighLogic.CurrentGame.Mode == Game.Modes.CAREER;
			}
		}

		internal static bool useReputations {
			get {
				return QSettings.Instance.Reputations && HighLogic.CurrentGame.Mode == Game.Modes.CAREER;
			}
		}

		internal static bool useSciences {
			get {
				return QSettings.Instance.Sciences && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX);
			}
		}

		internal static bool useRevertCost {
			get {
				return useCredits || useReputations || useSciences;
			}
		}

		internal static float Cost(RevertType RType, Currency CType, float vesselCost = -1) {
			if (useRevertCost) {
				double _cost = 0;
				double _defaultPrice = 0;
				double _factor = 1;
				if (RType == RevertType.LAUNCH) {
					_factor *= QSettings.Instance.RevertToLaunchFactor;
				} 
				switch (CType) {
				case Currency.Funds:
					if (!useCredits) {
						return 0;
					}
					_cost -= QSettings.Instance.CreditsCost * _factor;
					break;
				case Currency.Reputation:
					if (!useReputations) {
						return 0;
					}
					_cost -= QSettings.Instance.ReputationsCost * _factor;
					break;
				case Currency.Science:
					if (!useSciences) {
						return 0;
					}
					_cost -= QSettings.Instance.SciencesCost * _factor;
					break;
				}
				_defaultPrice -= _cost;
				if (QSettings.Instance.CostFctReputations) {
					_cost *= (1 - Reputation.UnitRep / 2);
				}
				if (QSettings.Instance.CostFctVessel) {
					float _vesselcost = (vesselCost == -1 ? VesselCost : vesselCost);
					if (_vesselcost > 0) {
						float _FctVessel = _vesselcost / (QSettings.Instance.VesselBasePrice > 0 ? QSettings.Instance.VesselBasePrice : 50000);
						_cost *= _FctVessel;
					}
				}
				if (QSettings.Instance.CostFctPenalties) {
					_cost *= HighLogic.CurrentGame.Parameters.Career.FundsLossMultiplier;
				}
				double _defaultCost = -0.1 * _defaultPrice;
				if (_cost > _defaultCost) {
					_cost = _defaultCost;
				}
				if (_cost > -1) {
					_cost = -1;
				}
				return Convert.ToSingle(Math.Round(_cost));
			} else {
				return 0;
			}
		}

		internal static float VesselCost {
			get {
				float _dryCost = 0, _fuelCost = 0, _VesselCost = 0;
				if (HighLogic.LoadedSceneIsEditor) {
					EditorLogic.fetch.ship.GetShipCosts (out _dryCost, out _fuelCost);
					return _dryCost + _fuelCost;
				} else if (HighLogic.LoadedSceneIsFlight) {
					if (FlightGlobals.ActiveVessel == null) {
						return 0;
					}
					ProtoVessel _pvessel = FlightGlobals.ActiveVessel.protoVessel;
					if (_pvessel == null) {
						return QSettings.Instance.VesselBasePrice;
					}
					List<ProtoPartSnapshot> _parts = _pvessel.protoPartSnapshots;
					foreach (ProtoPartSnapshot _part in _parts) {
						ShipConstruction.GetPartCosts (_part, _part.partInfo, out _dryCost, out _fuelCost);
						_VesselCost += _dryCost + _fuelCost;
					}
					return _VesselCost;
				}
				return 25000;
			}
		}

		internal static string msgCost(float credits, float reputations, float sciences, bool color = false) {
			string _string = string.Empty;
			string _tmp;
			if (useRevertCost) {
				if (useCredits) {
					if (color) {
						_tmp = " <color=#B4D455><b>{0}</b> funds</color>";
					} else {
						_tmp = " {0} funds";
					}
					_string += string.Format (_tmp, credits.ToString("N0"));
					if (useReputations && useSciences) {
						_string += ", ";
					} else if (useReputations || useSciences) {
						_string += " and";
					}
				}
				if (useReputations) {
					if (color) {
						_tmp = " <color=#E0D503><b>{0}</b> rep</color>";
					} else {
						_tmp = " {0} rep";
					}
					_string += string.Format (_tmp, reputations.ToString("N0"));
					if (useSciences) {
						_string += " and";
					}
				}
				if (useSciences) {
					if (color) {
						_tmp = " <color=#63b1d1><b>{0}</b> sci</color>";
					} else {
						_tmp = " {0} sci";
					}
					_string += string.Format (_tmp, sciences.ToString("N0"));
				}
				_string += ".";
			}
			return _string;
		}

		internal static bool GetIndex(ConfigNode nodes, out int iFunds, out int iReputations, out int iSciences, out float funds, out float reputations, out float sciences) {
			iFunds = iReputations = iSciences = -1;
			funds = reputations = sciences = -1;
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER && HighLogic.CurrentGame.Mode != Game.Modes.SCIENCE_SANDBOX) {
				return false;
			}
			if (!nodes.HasNode ("GAME")) {
				return false;
			}
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
				iFunds = Array.FindIndex (nodes.GetNode ("GAME").GetNodes ("SCENARIO"), n => n.GetValue ("name") == "Funding");
				funds = float.Parse (nodes.GetNode ("GAME").GetNode ("SCENARIO", iFunds).GetValue ("funds"));
				iReputations = Array.FindIndex (nodes.GetNode ("GAME").GetNodes ("SCENARIO"), n => n.GetValue ("name") == "Reputation");
				reputations = float.Parse (nodes.GetNode ("GAME").GetNode ("SCENARIO", iReputations).GetValue ("rep"));
			}
			iSciences = Array.FindIndex (nodes.GetNode ("GAME").GetNodes ("SCENARIO"), n => n.GetValue ("name") == "ResearchAndDevelopment");
			sciences = float.Parse (nodes.GetNode ("GAME").GetNode ("SCENARIO", iSciences).GetValue ("sci"));
			if (((funds == -1 || reputations == -1) && HighLogic.CurrentGame.Mode == Game.Modes.CAREER) || sciences == -1) {
				return false;
			}
			return true;
		}
			
		internal static int Pay(RevertType RType, out float creCost, out float repCost, out float sciCost) {
			creCost = repCost = sciCost = 0;
			GameBackup _gameBackup = (RType == RevertType.EDITOR ? FlightDriver.PreLaunchState : FlightDriver.PostInitState);
			if (_gameBackup == null) {
				return -1;
			}
			bool _return = true;
			int _iFunds, _iReputations, _iSciences;
			float _funds, _reputations, _sciences;
			if (GetIndex (_gameBackup.Config, out _iFunds, out _iReputations, out _iSciences, out _funds, out _reputations, out _sciences)) {
				if (useCredits) {
					creCost = Cost (RType, Currency.Funds);
					float _deltaCre = _funds + creCost;
					if (_deltaCre < 0) {
						_deltaCre = 0;
						_return &= (false || !QSettings.Instance.EnableRevertLoss);
					}
					_gameBackup.Config.GetNode ("GAME").GetNode ("SCENARIO", _iFunds).SetValue ("funds", _deltaCre.ToString ());
				}
				if (useReputations) {
					repCost = Cost (RType, Currency.Reputation);
					float _deltaRep = _reputations + repCost;
					_gameBackup.Config.GetNode ("GAME").GetNode ("SCENARIO", _iReputations).SetValue ("rep", _deltaRep.ToString ());
				}
				if (useSciences) {
					sciCost = Cost (RType, Currency.Science);
					float _deltaSci = _sciences + sciCost;
					if (_deltaSci < 0) {
						_deltaSci = 0;
						_return &= (false || !QSettings.Instance.EnableRevertLoss);
					}
					_gameBackup.Config.GetNode ("GAME").GetNode ("SCENARIO", _iSciences).SetValue ("sci", _deltaSci.ToString ());
				}
				return (_return ? 1 : 0);
			}
			return -1;
		}

		internal static void OnFlightReady() {
			if (!useRevertCost) {
				return;
			}
			#if KEEP
			if (QFlight.data.isSaved) {
				return;
			}
			#endif
			float _cre, _rep, _sci;
			int CanRevert = -1;
			CanRevert = Pay (RevertType.LAUNCH, out _cre, out _rep, out _sci);
			if (CanRevert == 1) {
				Quick.Warning ("Revert to Launch will cost you :" + msgCost (_cre, _rep, _sci));
			} else if (CanRevert == 0) {
				FlightDriver.CanRevertToPostInit = false;
				CanRevertToPostInit = false;
				ScreenMessages.PostScreenMessage (string.Format ("[{0}] You have no suffisant money to Revert to Launch.", Quick.MOD), 10, ScreenMessageStyle.UPPER_RIGHT);
				Quick.Warning ("Not enough fund/science to Revert to Launch");
			}
			CanRevert = Pay (RevertType.EDITOR, out _cre, out _rep, out _sci);
			if (CanRevert == 1) {
				Quick.Warning ("Revert to Editor will cost you :" + msgCost (_cre, _rep, _sci));
			} else if (CanRevert == 0) {
				FlightDriver.CanRevertToPrelaunch = false;
				CanRevertToPrelaunch = false;
				ScreenMessages.PostScreenMessage (string.Format ("[{0}] You have no suffisant money to Revert to Editor.", Quick.MOD), 10, ScreenMessageStyle.UPPER_RIGHT);
				Quick.Warning ("Not enough fund/science to Revert to Launch");
			}
		}

		internal static void OnRevertPopup(PopupDialog popup) {
			string _message = "Revert will cost you, for a:" + Environment.NewLine;
			if (FlightDriver.CanRevertToPrelaunch && FlightDriver.PreLaunchState != null) {
				string _msgCostLaunch = msgCost (Cost (RevertType.LAUNCH, Currency.Funds), Cost (RevertType.LAUNCH, Currency.Reputation), Cost (RevertType.LAUNCH, Currency.Science), true);
				_message += "- revert to launch:" + _msgCostLaunch + Environment.NewLine;
			}
			if (FlightDriver.CanRevertToPostInit && FlightDriver.PostInitState != null) {
				string _msgCostEditor = msgCost (Cost (RevertType.EDITOR, Currency.Funds), Cost (RevertType.EDITOR, Currency.Reputation), Cost (RevertType.EDITOR, Currency.Science), true);
				_message += "- revert to editor:" + _msgCostEditor + Environment.NewLine;

			}
			popup.dialogToDisplay.title = string.Format ("[{0}] Reverting Flight", Quick.MOD);
			popup.dialogToDisplay.message = _message;
		}
	}
}