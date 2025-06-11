using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework.UI;
using UnityEngine;
using Journeys.RedirectionFramework;
using Journeys.RedirectionFramework.Attributes;
using Journeys.RedirectionFramework.Extensions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


namespace Journeys
{

	//[TargetType(typeof(PathVisualizer))]

	public class JourneyVisualizer : MonoBehaviour
	{

		private bool m_journeysVisible;         // fundamental controller of whether simulation and render steps happen or not 

		public static JourneyVisualizer instance;
		public JourneyStepMgr theStepManager;   // there is of course only one of these, although I did not strictly enforce it by using the Singleton approach

		// these next are just shortcuts that are easier to read than Singleton<blah>.instance
		public readonly CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
		public readonly VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
		public readonly PathManager thePathManager = Singleton<PathManager>.instance;
		public readonly BuildingManager theBuildingManager = Singleton<BuildingManager>.instance;
		public readonly NetManager theNetManager = Singleton<NetManager>.instance;
		public readonly InstanceManager theInstanceManager = Singleton<InstanceManager>.instance;

		public Dictionary<ushort, Journey> m_journeys;      // holds all the journeys (=lists of steps), indexed by cim
		public bool DoneSegWays { get; set; }
		// these next two are empty if we have not DoneSegWays
		public Dictionary<ushort, SegWay> m_segways;        // for by-segment selections, a dictionary by segment number to Segways (ie: which cims use each segment, for all segments used by any cims)
		public Dictionary<ushort, List<Waypoint>> m_cimRoutes;    // mapping from cims to their Route (ie raw routes - not converting transport steps to land steps)
		public Dictionary<int, HashSet<ushort>> m_lineCims;				// map from line numbers (raw) to cims using that line

		public List<ushort> m_selectedSegments;    // primary by-segment selections (outlined in red)
		public List<ushort> m_selectedSegments2;   // secondary by-segment selections (outlined in green)
		public List<ushort> m_selectedBuildings;   // primary map selections of buildings (outlined in red)
		public List<ushort> m_selectedBuildings2;  // secondary map selections of buildings (outlined in green)
		public HashSet<ushort> m_primaryCims;      // cims selected by user making primary (red) map selections. It is a HashSet for the convenience of not explicitly checking for duplicates when adding to it
		public HashSet<ushort> m_secondaryRestrictedCims;    // a restriction of primaryCims, restricted by green selections (if none, then is not used)
		public HashSet<ushort> m_selectedCims;     // points to either primaryCims or to secondaryRestrictedCims, as appropriate. These are the cims who have "map-selected" journeys
		public HashSet<ushort> m_subselectedCims;   // set of cims actually showing after applying tourist, from-to and PT-stretch maskings (in ShowJourneys, flags set via Menu)
		public HashSet<ushort> m_restoreSubselected;  // a restore point for subselectedCims used in lane-line and ByJourney subselections

		private JourneyStepMgr.LaneLineSet m_LLTargetSteps;    // for lane-line cycling
		public List<ushort> m_BJcimsList;                      // for by-journey view restoration point
		public HashSet<ushort> m_BJtmpSet;                       // scratch set for passing
		private int m_BJendIndex;
		private int m_BJcimIndex;

		private InstanceID m_lastInstance;                     // previous map selection (shows in red or green), use is to identify if user has made a new selection since last sim step
		private readonly object buildLock = new object();     // pseudo object to lock on during build of journeys
		private readonly object renderLock = new object();
		private readonly object wipeLock = new object();
		private bool doneRender;
		private bool doneMeshes;
		public bool HeatMap { get; set; }
		public bool ShowOnlyTransportLanes { get; set; }
		public byte FromToFlag { get; set; }
		public bool OnlyPTstretches { get; set; }
		public bool ShowBlended { get; set; }
		public bool ByJourneyInitiated { get; private set; }
		public bool SubselectByLaneLineInitiated { get; set; }
		public bool LineMode { get; set; }
		public ushort SelectedLine { get; set; }
		public int MinHalfwidth { get; set; }
		public int TouristFlag { get; set; }                 // TouristFlag is 0 when neither Residents nor Tourists are selected (checked), 1 when residents only are selected and 2 when tourists only are selected
		public bool MakeSecondarySelection { get; set; }
		public bool ModeAdditionalSecondarySelection { get; set; }
		public bool MakeExtendedSelection { get; set; }
		public bool AfterAll { get; set; }

		public bool DoneRender
		{
			get
			{
				lock (renderLock)
				{
					return doneRender;
				}
			}
			private set
			{
				doneRender = value;
			}
		}

		public bool DoneMeshes
		{
			get
			{
				lock (buildLock)
				{
					return doneMeshes;
				}
			}
			private set
			{
				doneMeshes = value;
			}
		}


		//
		// ************************** initialization and destruction
		//

		private void Awake()
		{
			instance = this;
			//Debug.Log("JV: Awake has set instance");
			theStepManager = new JourneyStepMgr();
			m_journeys = new Dictionary<ushort, Journey>();
			m_primaryCims = new HashSet<ushort>();
			m_selectedCims = new HashSet<ushort>();
			m_restoreSubselected = new HashSet<ushort>();
			m_BJcimsList = new List<ushort>();
			m_journeysVisible = true;
			m_lastInstance = InstanceID.Empty;
			m_selectedSegments = new List<ushort>();
			m_selectedSegments2 = new List<ushort>();
			m_selectedBuildings = new List<ushort>();
			m_selectedBuildings2 = new List<ushort>();
			doneRender = true;                          // doneRender is always true, but DoneRender is locked during rendering (also just in case, RenderJourneys resets it true after rendering)
			doneMeshes = false;
			HeatMap = false;
			MinHalfwidth = 1;
			ShowBlended = false;
			ShowOnlyTransportLanes = true;
			FromToFlag = 0;
			TouristFlag = 0;
			ByJourneyInitiated = false;
			LineMode = false;
			SubselectByLaneLineInitiated = false;
			MakeSecondarySelection = false;
			ModeAdditionalSecondarySelection = false;
			MakeExtendedSelection = false;
			AfterAll = false;
			DoneSegWays = false;
			m_segways = new Dictionary<ushort, SegWay>();
			m_cimRoutes = new Dictionary<ushort, List<Waypoint>>();
			m_lineCims = new Dictionary<int, HashSet<ushort>>();
		}


		private void OnDestroy()
		{
			DestroyJourneys();
			UnityEngine.Debug.Log("JV: OnDestroy has been run");
		}

		//
		// ************************** the main call
		//


		public void SimulationStep(int subStep)
		{
			if (!m_journeysVisible)
				return;
			lock (buildLock)
			{
				// PV selections are shown dynamically using a m_pathRefreshFrame mechanism to check for updates,
				// but JV freezes journey info at the time of selection
				InstanceID clickedInstance = Singleton<InstanceManager>.instance.GetSelectedInstance();
				if (clickedInstance == null || clickedInstance == m_lastInstance || m_selectedSegments.Contains(clickedInstance.NetSegment) || m_selectedBuildings.Contains(clickedInstance.Building))
				{
					return;
				}
				if (!DoneSegWays)
				{
					SetSegWays();
					DoneSegWays = true;
				}
				if (MakeSecondarySelection)
				{
					AddSecondarySelection(clickedInstance);
					return;
				}
				m_lastInstance = clickedInstance;
				doneMeshes = false;
				if (!MakeExtendedSelection)
					WipeSlate();
				LineMode = false;
				ByJourneyInitiated = false;
				SubselectByLaneLineInitiated = false;
				m_selectedSegments2.Clear();
				m_selectedBuildings2.Clear();

				// for a citizen selection
				if (clickedInstance.Citizen != 0U)
				{
					ushort citizenInstanceID = theCitizenManager.m_citizens.m_buffer[clickedInstance.Citizen].m_instance;
					if (citizenInstanceID != 0 && m_cimRoutes.TryGetValue(citizenInstanceID, out List<Waypoint> rawroute))
					{
						AddJourney2(citizenInstanceID, rawroute);
					}
				}

				// for a vehicle selection
				else if (clickedInstance.Vehicle != 0)
				{
					bool carPath = false;
					int loopLimit = 0;
					ushort leadingVehicleID = theVehicleManager.m_vehicles.m_buffer[clickedInstance.Vehicle].GetFirstVehicle(clickedInstance.Vehicle);   // make sure we start with the leading vehicle for trams etc
					ushort vehicleID = leadingVehicleID;
					uint pathID = theVehicleManager.m_vehicles.m_buffer[leadingVehicleID].m_path;
					if (pathID != 0 && theVehicleManager.m_vehicles.m_buffer[leadingVehicleID].m_transportLine == 0)
						carPath = true;
					while (vehicleID != 0)
					{
						uint thisUnit = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_citizenUnits;
						int loopLimit2 = 0;
						while (thisUnit != 0)
						{
							for (int index = 0; index < 5; ++index)
							{
								uint citizen = theCitizenManager.m_units.m_buffer[thisUnit].GetCitizen(index);
								if (citizen != 0)
								{
									ushort citInst = theCitizenManager.m_citizens.m_buffer[citizen].m_instance;
									uint citPathID = theCitizenManager.m_instances.m_buffer[citInst].m_path;
									if (carPath)
									{
										if (pathID != 0)
											AddJourney2(citInst, JVutils.PathToWaypoints(pathID));
									}
									else if (citPathID != 0)
										AddJourney2(citInst, JVutils.PathToWaypoints(citPathID));
								}
							}
							thisUnit = theCitizenManager.m_units.m_buffer[thisUnit].m_nextUnit;
							if (++loopLimit2 > 524288)
							{
								UnityEngine.Debug.LogError("JV Error: Invalid list of citizen units detected!\n" + System.Environment.StackTrace);
								break;
							}
						}
						vehicleID = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
						if (++loopLimit > 16384)
						{
							UnityEngine.Debug.LogError("JV Error: Invalid list of leading/trailing vehicles detected!\n" + System.Environment.StackTrace);
							break;
						}
					}
				}

				// for a building selection
				else if (clickedInstance.Building != 0)
				{
					ushort buildingID = clickedInstance.Building;
					m_selectedBuildings.Add(buildingID);
					List<ushort> newSegList = new List<ushort>();
					GetBuildingSegments(buildingID, newSegList);
					AddSegmentJourneys(newSegList);
					AddBuildingJourneys(buildingID);

				}

				// for a segment selection (including an extended selection)
				else if (clickedInstance.NetSegment != 0)
				{
					ushort newSegment = clickedInstance.NetSegment;
					m_selectedSegments.Add(newSegment);
					if (m_segways.TryGetValue(newSegment, out var newSegway))
					{
						foreach (ushort cim in newSegway.m_cims)
						{
							AddJourney2(cim, m_cimRoutes[cim]);
						}
					}
				}

				// for a line selection
				else if (clickedInstance.TransportLine != 0)
				{
					LineMode = true;
					SelectedLine = clickedInstance.TransportLine;
					m_selectedSegments.Clear();
					AddLineJourneys(SelectedLine);
				}


				// for everything after the above
				Singleton<JourneysPanel>.instance.UpdateBothSelected(m_primaryCims.Count, m_selectedCims.Count);
				if (m_selectedSegments.Count == 1)
					Singleton<JourneysPanel>.instance.LaneLineEnable();
				else
					Singleton<JourneysPanel>.instance.LaneLineDisable();
				m_selectedCims = m_primaryCims;
				if (TouristFlag == 0 && FromToFlag == 0 && OnlyPTstretches == false)
				{
					m_subselectedCims = m_primaryCims;
					theStepManager.ResetMeshes();
					Singleton<JourneysPanel>.instance.UpdateBothSelected(m_primaryCims.Count, m_subselectedCims.Count);
				}
				else
				{
					ShowJourneys(m_selectedCims);
				}
				if (AfterAll)
				{
					Singleton<JourneysPanel>.instance.AfterAll();
					AfterAll = false;
				}

				doneMeshes = true;
			}
		}


		private void GetBuildingSegments(ushort buildingID, List<ushort> segmentList)
		{
			int loopLimit = 0;
			while (buildingID != 0)
			{
				// we look for segments associated with the building node(s), and add them to the argument segment list
				// this picks up, for example, the platform segments of metro stations
				// initialize the netNode on which the building sits
				ushort targetNetNode = theBuildingManager.m_buildings.m_buffer[buildingID].m_netNode;
				int loopLimit2 = 0;
				while (targetNetNode != 0)
				{
					//Debug.Log("targetNetNode" + targetNetNode + " m_nextBuildingNode " + theNetManager.m_nodes.m_buffer[targetNetNode].m_nextBuildingNode);
					// Exclude public transport nodes here because we pick up from the land-based segments, not the PT segments
					if (theNetManager.m_nodes.m_buffer[targetNetNode].Info.m_class.m_layer != ItemClass.Layer.PublicTransport)
					{
						// check all 8 segments coming from this node (I assume some or most are null) - really check all 8 there is no break or continue
						for (int index = 0; index < 8; ++index)
						{
							ushort segment = theNetManager.m_nodes.m_buffer[targetNetNode].GetSegment(index);
							// it the segment starts at the target node (and is not null and flags are ok - it needs to be untouchable, whatever that means)
							if (segment != 0 && theNetManager.m_segments.m_buffer[segment].m_startNode == targetNetNode && (theNetManager.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
							{
								segmentList.Add(segment);
							}
						}
					}
					targetNetNode = theNetManager.m_nodes.m_buffer[targetNetNode].m_nextBuildingNode;     // loop again for the next node of a big building that has multiple nodes
					if (++loopLimit2 > 32768)
					{
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid targetNetNode list detected!\n" + System.Environment.StackTrace);
						break;
					}
				}
				buildingID = theBuildingManager.m_buildings.m_buffer[buildingID].m_subBuilding;   // outer loop again for any subbuilding of the target building
				if (++loopLimit > 49152)
				{
					CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid subbuilding list detected!\n" + System.Environment.StackTrace);
					break;
				}
			}
		}

		// AddSegmentJourneys is just for adding the journeys on segments within a building (eg the platforms in a tube station)
		// it's not used generically for adding all segment journeys in all circumstances

		private void AddSegmentJourneys(List<ushort> segList)
		{
			foreach (ushort seg in segList)
			{
				if (m_segways.TryGetValue(seg, out var nextSegway))
				{
					foreach (ushort cim in nextSegway.m_cims)
					{
						AddJourney2(cim, m_cimRoutes[cim]);
					}
				}
			}
		}

		//// this next bit from PV first detects if a citizen's target building is in the m_targets list, if a building is selected
		//// which would not get picked up otherwise because there would not be a segment selected to find a hit
		//// (except for selected metro and train stations, which do select segments - but they would not be a citizen's target)
		//InstanceID targetId = thisCitInst.Info.m_citizenAI.GetTargetID((ushort)citant, ref thisCitInst);
		//addable |= m_targets.Contains(targetId);
		//if (targetId.Building != 0)
		//{
		//    Vector3 position = theBuildingManager.m_buildings.m_buffer[targetId.Building].m_position;
		//    InstanceID newID = InstanceID.Empty;
		//    newID.District = theDistrictManager.GetDistrict(position);
		//    addable |= m_targets.Contains(newID);
		//    newID.Park = theDistrictManager.GetPark(position);
		//    addable |= m_targets.Contains(newID);
		//}
		//if (targetId.NetNode != 0)
		//{
		//    Vector3 position = theNetManager.m_nodes.m_buffer[targetId.NetNode].m_position;
		//    InstanceID newID = InstanceID.Empty;
		//    newID.District = theDistrictManager.GetDistrict(position);
		//    addable |= m_targets.Contains(newID);
		//    newID.Park = theDistrictManager.GetPark(position);
		//    addable |= m_targets.Contains(newID);
		//}


		private void AddBuildingJourneys(ushort buildingID)
		{
			int loopLimit = 0;
			while (buildingID != 0)
			{
				int loopLimit2 = 0;
				Building theBuilding = theBuildingManager.m_buildings.m_buffer[buildingID];
				ushort cim = theBuilding.m_sourceCitizens;
				while (cim != 0)
				{
					AddJourney(cim);
					cim = theCitizenManager.m_instances.m_buffer[cim].m_nextSourceInstance;
					if (++loopLimit2 > 1000)
					{
						UnityEngine.Debug.LogError("JV Error: Invalid linked list of citizen units detected in building!\n" + System.Environment.StackTrace);
						break;
					}
				}
				loopLimit2 = 0;
				cim = theBuilding.m_targetCitizens;
				while (cim != 0)
				{
					AddJourney(cim);
					cim = theCitizenManager.m_instances.m_buffer[cim].m_nextTargetInstance;
					if (++loopLimit2 > 1000)
					{
						UnityEngine.Debug.LogError("JV Error: Invalid linked list of citizen units detected in building!\n" + System.Environment.StackTrace);
						break;
					}
				}
				buildingID = theBuildingManager.m_buildings.m_buffer[buildingID].m_subBuilding;   // outer loop again for any subbuilding of the target building
				if (++loopLimit > 49152)
				{
					CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid subbuilding list detected!\n" + System.Environment.StackTrace);
					break;
				}
			}
		}


		// AddJourney is where a new Journey is created and added to m_journeys
		// Creating the journey also creates and populates the corresponding JourneySteps, including their meshes
		private void AddJourney2(ushort cim, List<Waypoint> rawRoute, bool checkedUnique = false)
		{
			UnityEngine.Debug.LogError("JV got here ok - 1");
			if (rawRoute == null || rawRoute.Count < 2)
			{
				UnityEngine.Debug.LogError("JV adding null or short route length ");
				return;
			}
			if (checkedUnique || !m_journeys.ContainsKey(cim))
			{
				UnityEngine.Debug.LogError("JV got here - 2");
				Journey journey = new Journey(rawRoute, cim);
				UnityEngine.Debug.LogError("JV got here - 2.5");
				lock (journey)
					journey.SetSteps();
				UnityEngine.Debug.LogError("JV got here - 3");
				if (journey.m_steps.Count > 0)
				{
					m_journeys.Add(cim, journey);
					m_primaryCims.Add(cim);
				}
			}
		}


		private void AddJourney(ushort cim, bool checkedUnique = false)
		{
			if (checkedUnique || !m_journeys.ContainsKey(cim))
			{
				uint path = GetCitizenPath(cim);
				if (path != 0)
					AddJourney2(cim, JVutils.PathToWaypoints(path), checkedUnique: true);
			}
		}

		private uint GetCitizenPath(ushort cim)
		{
			CitizenInstance cimInst = theCitizenManager.m_instances.m_buffer[cim];
			uint path = cimInst.m_path;
			if (path == 0)          // if path is zero, check for the path being held in the citizen's car
			{
				uint citizen = cimInst.m_citizen;
				ushort vehicleID = theCitizenManager.m_citizens.m_buffer[citizen].m_vehicle;
				Vehicle vehicle = theVehicleManager.m_vehicles.m_buffer[vehicleID];
				if (vehicle.Info.m_vehicleType == VehicleInfo.VehicleType.Car)
					path = vehicle.m_path;
			}
			return path;
		}

		private void AddLineJourneys(ushort line)
		{
			foreach (ushort cim in m_lineCims[line])
			{
				AddJourney2(cim, m_cimRoutes[cim], checkedUnique: true);
			}
		}

		private void AddSecondarySelection(InstanceID clickedInstance)
		{
			// I am not sure null can happen, but failsafe
			if (clickedInstance == null)
				return;
			// soon later a comprehensive test whether the click was on any of the existing selections
			ushort newSegmentID = clickedInstance.NetSegment;    // this will be zero if anything other than a segment was selected
			ushort newBuildingID = clickedInstance.Building;
			List<ushort> newSegments = new List<ushort>();
			if (newSegmentID == 0)
			{
				if (newBuildingID == 0)
				{
					theInstanceManager.ReleaseInstance(clickedInstance);
					return;             // attempted secondary selections of anything other than a segment or a building are ignored
				}
				else
				{
					if (m_selectedBuildings.Contains(newBuildingID) || m_selectedBuildings2.Contains(newBuildingID))        // ignore attempt to select the same building as any already selected
					{
						theInstanceManager.ReleaseInstance(clickedInstance);
						return;
					}
					if (!ModeAdditionalSecondarySelection)
					{
						m_selectedBuildings2.Clear();
						m_selectedSegments2.Clear();
					}
					GetBuildingSegments(newBuildingID, newSegments);
					// a building without any segments is disallowed as a secondary selection
					if (newSegments.Count > 0)
					{
						m_selectedSegments2.AddRange(newSegments);
						m_selectedBuildings2.Add(newBuildingID);
					}
					else
					{
						theInstanceManager.ReleaseInstance(clickedInstance);
						return;
					}
				}
			}
			else
			{
				if (m_selectedSegments.Contains(newSegmentID) || m_selectedSegments2.Contains(newSegmentID))        // ignore attempt to select the same segment as any already selected
				{
					theInstanceManager.ReleaseInstance(clickedInstance);
					return;
				}
				if (!ModeAdditionalSecondarySelection)
				{
					m_selectedBuildings2.Clear();
					m_selectedSegments2.Clear();
				}
				newSegments.Add(newSegmentID);
				m_selectedSegments2.Add(newSegmentID);
			}
			doneMeshes = false;
			LineMode = false;
			HashSet<ushort> newSteps2 = new HashSet<ushort>();
			theStepManager.EnsureSelectedSteps(m_selectedSegments, m_selectedSegments2, newSteps2, setNewSteps: true);
			m_secondaryRestrictedCims = theStepManager.GetRestrictedCims(newSteps2, m_selectedCims);
			m_selectedCims = m_secondaryRestrictedCims;
			ShowJourneys(m_selectedCims);
			SubselectByLaneLineInitiated = false;
			doneMeshes = true;
		}

		public void SaveOrRestoreSelection()
		{
			if (!ModeAdditionalSecondarySelection)
				RestoreSelection();
		}

		public void RestoreSelection()
		{
			m_selectedCims = m_primaryCims;
			m_selectedBuildings2.Clear();
			m_selectedSegments2.Clear();
			// the following contortions stop simulation step treating last green selection (still = clickedInstance) as a new primary selection
			// by making one of the red segments/buildings be selected. Failsafe m_lastInstance is set to Selected, so sim step does not treat
			// Selected as a new selection, but this prevents user selecting what was just green (until they make a different selection)
			InstanceID redInst = InstanceID.Empty;
			if (m_selectedSegments.Count > 0)
				redInst.NetSegment = m_selectedSegments[0];
			else
				redInst.Building = m_selectedBuildings[0];
			if (!Singleton<InstanceManager>.instance.SelectInstance(redInst))
				m_lastInstance = Singleton<InstanceManager>.instance.GetSelectedInstance();
			ShowJourneys(m_selectedCims);
		}

		public void ToggleHeatMap()
		{
			HeatMap = !HeatMap;
		}

		// CallReheat resets the currently selected meshes to new heats, which are then picked up by Render ticks. It does not change any step selection
		public void CallReheat()
		{
			lock (buildLock)
			{
				doneMeshes = false;
				theStepManager.ResetMeshes();
				doneMeshes = true;
			}
		}

		public void ToggleTransportSteps()
		{
			ShowOnlyTransportLanes = !ShowOnlyTransportLanes;
		}

		public void ToggleShowPTstretches()
		{
			OnlyPTstretches = !OnlyPTstretches;
			ShowJourneys(m_selectedCims);
		}

		public void ToggleShowBlended()
		{
			ShowBlended = !ShowBlended;
			CallReheat();
		}

		// ShowJourneys is the key function to restrict the showing journeys from the original selection to a subselection.
		// It currently takes a very simplistic approach in that it first hides all journeys, then restores those in the subselection.
		// This is clearly massively inefficient if only a few journeys should end up hidden - it would then obviously be better to leave everything showing
		// and selectively hide those that have been de-selected.  Nevertheless, even with 10000 journeys, this step is quasi-instantaneous, the
		// inefficiency is not a practical issue.  Also to note, HideAllCims is extremely quick for each step, especially if already all cims are hidden
		public void ShowJourneys(HashSet<ushort> baseList)
		{
			lock (buildLock)
			{
				CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
				List<ushort> maskedList;

				theStepManager.HideAllCims();
				UnityEngine.Debug.Log("baseList count is " + baseList.Count());
				if (baseList.Count == 0)
				{
					Singleton<JourneysPanel>.instance.UpdateBothSelected(m_selectedCims.Count, 0);
					return;
				}
				doneMeshes = false;
				HashSet<ushort> dummy = new HashSet<ushort>();
				theStepManager.EnsureSelectedSteps(m_selectedSegments, m_selectedSegments2, dummy);
				UnityEngine.Debug.Log("length of selected steps is " + theStepManager.m_primarySteps.Count());
				m_subselectedCims = new HashSet<ushort>();
				foreach (ushort cim in baseList)
				{
					if (MaskTourist(cim))
					{
						maskedList = MaskPT(MaskFromTo(m_journeys[cim].m_steps, cim), cim);
						if (maskedList.Count > 0)
						{
							m_subselectedCims.Add(cim);
							foreach (ushort stepIdx in maskedList)
								theStepManager.GetStep(stepIdx).ShowCitizen(cim);
						}
					}
				}
				Singleton<JourneysPanel>.instance.UpdateBothSelected(m_selectedCims.Count, m_subselectedCims.Count);
				theStepManager.ResetMeshes();
				doneMeshes = true;
			}
		}

		public bool MaskTourist(ushort cim)
		{
			if (TouristFlag == 0)
				return true;
			bool tourist = theCitizenManager.m_instances.m_buffer[cim].Info.m_class.m_service == ItemClass.Service.Tourism;
			return (TouristFlag == 1 && !tourist) || (TouristFlag == 2 && tourist);
		}

		public List<ushort> MaskFromTo(List<ushort> stepList, ushort cim)
		{
			if (stepList.Count <= 1)
				return stepList;
			if (m_selectedSegments2.Count > 0)
			{
				if (FromToFlag == 0)
				{
					return stepList;
				}
				else
				{
					for (int idx = 0; idx < stepList.Count; idx++)
					{
						if (theStepManager.m_primarySteps.Contains(stepList[idx]))  // if we hit A first
						{
							if (FromToFlag == 1)                        // if we are doing "From here", include this (whole) journey
								return stepList;
							else
								return new List<ushort>();
						}
						else if (theStepManager.m_secondarySteps.Contains(stepList[idx]))
						{
							if (FromToFlag == 2)
								return stepList;
							else
								return new List<ushort>();
						}
					}
				}
				return new List<ushort>();      // this can occur for eg m_subtargetSteps is empty for some reason
			}
			else
			{
				if (FromToFlag == 1)            // for "From here"
				{
					for (int idx = stepList.Count - 1; idx >= 0; idx--)
						if (theStepManager.m_primarySteps.Contains(stepList[idx]))
							return stepList.GetRange(idx, stepList.Count - idx);
					if (m_selectedBuildings.Count > 0 && m_selectedBuildings.Contains(theCitizenManager.m_instances.m_buffer[cim].m_sourceBuilding))
						return stepList;
				}
				else if (FromToFlag == 2)       // for "To here"
				{
					for (int idx = 0; idx < stepList.Count; idx++)
						if (theStepManager.m_primarySteps.Contains(stepList[idx]))
							return stepList.GetRange(0, idx + 1);
					if (m_selectedBuildings.Count > 0 && m_selectedBuildings.Contains(theCitizenManager.m_instances.m_buffer[cim].m_targetBuilding))
						return stepList;
				}
				else
				{
					return stepList;
				}
				return new List<ushort>(); // deliberately fall into this (or hit it as failsafe)
			}
		}

		public List<ushort> MaskPT(List<ushort> stepList, ushort cim)
		{
			if (!OnlyPTstretches || stepList.Count == 0)
				return stepList;
			List<ushort> maybeConnections = new List<ushort>();
			List<ushort> retList = new List<ushort>();
			bool caughtPT = false;
			foreach (ushort stepIdx in stepList)
			{
				JourneyStep jStep = theStepManager.GetStep(stepIdx);
				if (jStep.IsTransportStep(cim))
				{
					retList.AddRange(maybeConnections);
					retList.Add(stepIdx);
					maybeConnections.Clear();
					caughtPT = true;
				}
				else
				{
					if (caughtPT)
					{
						maybeConnections.Add(stepIdx);
					}
				}
			}
			return retList;
		}

		public void SubselectByLaneLine(bool forwards = true)
		{
			// this function makes no sense for multiple (or no) selected segments, nor for selected transport lines, so throw out in these cases
			if (LineMode || m_selectedSegments.Count != 1)
				return;
			HashSet<ushort> dummy = null;
			if (theStepManager.m_primarySteps.Count == 0)
				theStepManager.EnsureSelectedSteps(m_selectedSegments, m_selectedSegments2, dummy);
			if (theStepManager.m_primarySteps.Count == 0)
				return;
			lock (buildLock)
			{
				doneMeshes = false;     // not entirely true but stops RenderJourneys in the meantime
				if (!SubselectByLaneLineInitiated)
				{
					m_LLTargetSteps = new JourneyStepMgr.LaneLineSet(theStepManager.m_primarySteps, m_selectedSegments[0]);
					SubselectByLaneLineInitiated = true;
				}

				ShowJourneys(m_LLTargetSteps.GetNextLaneLineCims(forwards, ShowOnlyTransportLanes));
				doneMeshes = true;
			}
		}


		public void ByJourney(bool forwards = true)
		{
			HashSet<ushort> tmpSet;
			lock (buildLock)
			{
				doneMeshes = false;
				if (m_subselectedCims.Count == 0)
					return;
				if (!ByJourneyInitiated)
				{
					m_BJcimsList = m_subselectedCims.ToList();
					m_BJendIndex = m_BJcimsList.Count;
					m_BJcimIndex = forwards ? 0 : m_BJendIndex - 1;
					ByJourneyInitiated = true;
					Singleton<JourneysPanel>.instance.LaneLineDisable();            // whatever you do next (eg make a new selection) re-enables lane-line if appropriate
				}

				if (m_BJcimIndex == m_BJendIndex)
				{
					tmpSet = new HashSet<ushort>(m_BJcimsList);
					m_BJcimIndex = forwards ? 0 : m_BJendIndex - 1;
				}
				else
				{
					tmpSet = new HashSet<ushort>() { m_BJcimsList[forwards ? m_BJcimIndex++ : m_BJcimIndex--] };
					if (m_BJcimIndex < 0)
						m_BJcimIndex = m_BJendIndex;
				}
			}
			ShowJourneys(tmpSet);
			doneMeshes = true;
		}

		public void ShowAllJourneys()
		{
			lock (buildLock)
			{
				doneMeshes = false;
				WipeSlate();
				ByJourneyInitiated = false;
				LineMode = false;
				SubselectByLaneLineInitiated = false;
				MakeSecondarySelection = false;
				ModeAdditionalSecondarySelection = false;
				MakeExtendedSelection = false;

				if (!DoneSegWays)
				{
					SetSegWays();
					DoneSegWays = true;
				}
				foreach (ushort cim in m_cimRoutes.Keys)
				{
					AddJourney2(cim, m_cimRoutes[cim], true);
				}
				FromToFlag = 0;
				OnlyPTstretches = false;
				TouristFlag = 0;
				if (theStepManager.StepCount > 0)
					theStepManager.ResetMeshes();
				else
					UnityEngine.Debug.LogError("all journeys now finds stepcount " + theStepManager.StepCount);  // this should never happen!
				m_selectedCims = m_primaryCims;
				m_subselectedCims = m_primaryCims;
				Singleton<JourneysPanel>.instance.UpdateBothSelected(m_selectedCims.Count, m_subselectedCims.Count);
				AfterAll = true;    // flag to re-enable selector buttons

				doneMeshes = true;
			}
		}

		// m_journeysVisible is this is the fundamental controller of whether SimulationStep happens or not (as well as the obvious meaning)
		// I thought there might be calls to PathsVisible from other managers, but there do not seem to be.  Still, I left it here for now.
		public bool PathsVisible
		{
			get
			{
				UnityEngine.Debug.Log("JV: called PathsVisible.get with value = " + m_journeysVisible);
				return m_journeysVisible;
			}
			set
			{
				UnityEngine.Debug.Log("JV: Called PathsVisible.set with value = " + value);
				m_journeysVisible = value;
			}
		}

		public void DestroyJourneys()
		{
			if (DoneMeshes && DoneRender)
			{
				theStepManager.DestroyAll();
				DoneMeshes = false;         // this stops a wasted RenderJourneys
			}
			lock (m_journeys)
			{
				m_journeys.Clear();
			}
			lock (m_segways)
				m_segways.Clear();
			lock (m_cimRoutes)
				m_cimRoutes.Clear();
			DoneSegWays = false;
		}


		// RenderJourneys is called from outside JV in the manager sim steps at high level
		// (by redirection from PV.RenderPaths)
		public void RenderJourneys(RenderManager.CameraInfo cameraInfo, int layerMask)
		{
			if (!m_journeysVisible || !DoneMeshes)
				return;
			lock (renderLock)
			{
				foreach (ushort seg in m_selectedSegments)
					JVutils.DrawSegmentOverlay(cameraInfo, seg, Color.red, alphaBlend: true);
				foreach (ushort seg in m_selectedSegments2)
					JVutils.DrawSegmentOverlay(cameraInfo, seg, Color.green, alphaBlend: true);
				foreach (ushort bldg in m_selectedBuildings)
					JVutils.DrawBuildingOverlay(cameraInfo, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[bldg], Color.red);
				foreach (ushort bldg in m_selectedBuildings2)
					JVutils.DrawBuildingOverlay(cameraInfo, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[bldg], Color.green);
				theStepManager.DrawTheMeshes(cameraInfo, layerMask);
				doneRender = true;
			}
		}

		public void WipeSlate()
		{
			lock (wipeLock)
			{
				theStepManager.WipeSlate();
				lock (m_journeys)
					m_journeys.Clear();
				m_primaryCims.Clear();
				m_selectedCims.Clear();
				m_selectedSegments.Clear();
				m_selectedSegments2.Clear();
				m_selectedBuildings.Clear();
				m_selectedBuildings2.Clear();
			}
		}

		// network manager insists on calling this, so it must exist. But it's only called on quitting the PV
		public void UpdateData()
		{
			UnityEngine.Debug.Log("JV: called UpdateData");
		}


		// IsPathVisble is a helper function called by ALL the vehicleAI classes individually, to determine if the
		// vehicle should be drawn the same colour as its path (makes sense for PV, not always for JV)

		public bool IsPathVisible(InstanceID id)
		{
			return false;
		}

		public void SetSegWays()
		{
			// for all CitizenInstances that have been created but are are not marked as Deleted or WaitingPath
			for (int cimi = 0; cimi < 65536; ++cimi)
			{
				CitizenInstance thisCitInst = theCitizenManager.m_instances.m_buffer[cimi];
				if ((thisCitInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
					continue;
				ushort cim = (ushort)cimi;
				uint pathID = GetCitizenPath(cim);
				List<Waypoint> rawRoute = JVutils.PathToWaypoints(pathID);
				if (rawRoute == null || rawRoute.Count < 2)
					continue;
				m_cimRoutes.Add(cim, rawRoute);
				foreach (Waypoint wpoint in rawRoute)
				{
					if (!m_segways.TryGetValue(wpoint.Segment, out SegWay segway))
					{
						segway = new SegWay(wpoint.Segment);
						m_segways.Add(wpoint.Segment, segway);
					}
					if (segway.IsTransport)
					{
						if (!m_lineCims.ContainsKey(segway.m_line)) 
						{
							HashSet<ushort> hs = new HashSet<ushort>();
							m_lineCims[segway.m_line] = hs;
						}
						m_lineCims[segway.m_line].Add(cim);
						if (segway.TransportSegWaysSet)
						{
							foreach (Waypoint landpoint in segway.m_landroute)
							{
								if (!m_segways.TryGetValue(landpoint.Segment, out SegWay landsegway))
									UnityEngine.Debug.Log("should not be here 1a");
								else if (landsegway.m_cims == null)
									UnityEngine.Debug.Log("should not be here 1b");
								m_segways[landpoint.Segment].AddCimUnsafe(cim);
							}

						}
						else
						{	
							foreach (Waypoint landpoint in segway.m_landroute)
							{
								if (!m_segways.TryGetValue(landpoint.Segment, out SegWay landsegway))
								{
									landsegway = new SegWay(landpoint.Segment, knownLand: true);
									m_segways.Add(landpoint.Segment, landsegway);
								}
								if (landsegway.m_cims == null)
									UnityEngine.Debug.Log("should not be here 2");
								landsegway.AddCimUnsafe(cim);
							}
							segway.TransportSegWaysSet = true;
						}
					}
					else
					{
						if (segway.m_cims == null)
							UnityEngine.Debug.Log("should not be here 3");
						segway.AddCimUnsafe(cim);
					}
				}
			}
		}
	}
}


