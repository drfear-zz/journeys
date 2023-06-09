using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Journeys
{
    // this manager is not strictly a Manager in the game sense, because it is not created as a Singleton and is not
    // added to the game's list of Managers. Instead just one single instance is created as a member of JourneyVisualizer
    public class JourneyStepMgr
	{
        [NonSerialized]
        private Dictionary<ushort, JourneyStep> m_StepIndex;
        [NonSerialized]
        private Dictionary<string, ushort> m_HashToIdx;
        private Stack<ushort> m_indexStack;
        public HashSet<ushort> m_primarySteps;         // steps that are along, or partly along, a primary selected segment (used in From-To masking and lane-line selection)
        public HashSet<ushort> m_secondarySteps;       // steps that are along, or partly along, a secondary selected segment (used in From-To masking)

        public int StepCount => m_StepIndex.Count;
        private bool InitializedPrimarySteps { get; set; }
        private bool InitializedSecondarySteps { get; set; }

        private readonly object mgrLock = new object();

        public JourneyStepMgr()
        {
            m_StepIndex = new Dictionary<ushort, JourneyStep>();
            m_HashToIdx = new Dictionary<string, ushort>();
            m_indexStack = new Stack<ushort>();
            m_indexStack.Push(1);
            m_primarySteps = new HashSet<ushort>();
            m_secondarySteps = new HashSet<ushort>();
            InitializedPrimarySteps = false;
            InitializedSecondarySteps = false;
        }

        public void DestroyAll()
        {
            lock (mgrLock)
            {
                m_StepIndex = null;
                m_HashToIdx = null;
                m_indexStack = null;
            }
        }

        public void WipeSlate()
        {
            lock (mgrLock)
            {
                lock (m_StepIndex)
                {
                    m_StepIndex.Clear();
                    m_HashToIdx.Clear();
                    m_indexStack.Clear();
                    m_indexStack.Push(1);
                    m_primarySteps.Clear();
                    m_secondarySteps.Clear();
                    InitializedPrimarySteps = false;
                    InitializedSecondarySteps = false;
                }
            }
        }

        public void HideAllCims()
        {
            lock (mgrLock)
            {
                foreach (JourneyStep jstep in m_StepIndex.Values)
                {
                    jstep.HideAllCims();
                }
            }
        }


        public void EnsureSelectedSteps(List<ushort> primarySegs, List<ushort> secondarySegs, HashSet<ushort> newSecondarySteps,
            bool resetPrimary = false, bool appendPrimary = false, bool resetSecondary = false, bool appendSecondary = false, bool setNewSteps = false)
        {
            bool doPrimary = false;
            bool doSecondary = false;
            if (resetPrimary || !InitializedPrimarySteps)
            {
                m_primarySteps.Clear();
                doPrimary = true;
            }
            else if (appendPrimary)
                doPrimary = true;
            if (resetSecondary || !InitializedSecondarySteps)
            {
                m_secondarySteps.Clear();
                doSecondary = true;
            }
            else if (appendSecondary)
                doSecondary = true;
            if (!doPrimary && !doSecondary)
                return;

            if (setNewSteps)
                newSecondarySteps.Clear();
            foreach (KeyValuePair<ushort, JourneyStep> pair in m_StepIndex)
            {
                JourneyStep jstep = pair.Value;
                ushort stepNum = pair.Key;
                if (doPrimary && (primarySegs.Contains(jstep.StartStep.Segment) || primarySegs.Contains(jstep.EndStep.Segment)))
                {
                    m_primarySteps.Add(stepNum);
                    continue;
                }
                if (doSecondary && (secondarySegs.Contains(jstep.StartStep.Segment) || secondarySegs.Contains(jstep.EndStep.Segment)))
                {
                    if (!m_secondarySteps.Contains(stepNum))
                    {
                        m_secondarySteps.Add(stepNum);
                        if (setNewSteps)
                            newSecondarySteps.Add(stepNum);
                    }
                }
            }
            InitializedPrimarySteps = true;
            InitializedSecondarySteps = true;
        }
            

        // this function returns a list of cims that hit a list of green selections (in terms of steps in stepList), as well as already being selected (in currentSelectedCims)
        // it is used to create new subselectedCims according to a new green selection, constrained to cims who must already be in the current selection
        // The function could in theory have other uses, but not in JV
        public HashSet<ushort> GetRestrictedCims(HashSet<ushort> stepList, HashSet<ushort> currentSelectedCims)
        {
            HashSet<ushort> cimHash = new HashSet<ushort>();
            foreach (ushort stepNum in stepList)
            {
                foreach (ushort cim in m_StepIndex[stepNum].m_CimLines.Keys)
                {
                    if (currentSelectedCims.Contains(cim))
                        cimHash.Add(cim);
                }

            }
            return cimHash;
        }


        public List<ushort> GetTargetSteps()
        {
            lock (mgrLock)
            {
                return m_primarySteps.ToList();
            }
        }
        public List<ushort> GetTarget2Steps()
        {
            lock (mgrLock)
            {
                return m_secondarySteps.ToList();
            }
        }

        public JourneyStep GetStep(ushort idx)
        {
            lock (mgrLock)
            {
                return m_StepIndex[idx];
            }
        }

        // Augment will either add a new JStep for a new step, for a given citizen journey, or increment an existing step
        // it returns a list of the ushort indices of the new (or pre-existing) step(s)
        public List<ushort> Augment(Waypoint pointA, Waypoint pointB, ushort citizenID, int travelMode, Color lineColor, bool endJourney = false, bool show = true,
            bool skipCornerCheck = false)
        {
            lock (mgrLock)
            {
                List<ushort> outlist = new List<ushort>();
                pointA.Rationalize();
                pointB.Rationalize();
                // rationalize PT offsets to 0, 128 and 255
                // check (unless check switched off, which it is for recursive calls) if turn a corner
                // break the route into 2 steps if so (reason is to have overall finer tuning of heats)
                if (!skipCornerCheck && JVutils.TurnsCorner(pointA, pointB, out Waypoint pointBprime))
                {
                    outlist.AddRange(Augment(pointA, pointBprime, citizenID, travelMode, lineColor, false, show, skipCornerCheck: true));
                    outlist.AddRange(Augment(pointBprime, pointB, citizenID, travelMode, lineColor, endJourney, show, skipCornerCheck: true));
                }
                else
                {
                    // see if we need to create a new step, or add to existing
                    string hashname = Hashname(pointA, pointB);
                    if (m_HashToIdx.TryGetValue(hashname, out ushort uindex))
                    {
                        m_StepIndex[uindex].AugmentStepData(citizenID, travelMode, lineColor);
                        outlist.Add(uindex);
                    }
                    else
                    {
                        ushort newindex = GetNewIndex();
                        // adjustment for journeys ending exactly on end of segment, make them not quite do so, so makes a separate step to those who continue on
                        // better, of course, would be to refine or redesign steps for better merging pedestrians (in particular), but hey ho ...
                        if (endJourney)
                            if (pointB.Offset == 0)
                                pointB.Offset = 1;
                            else if (pointB.Offset == 255)
                                pointB.Offset = 254;
                        JourneyStep newStep = new JourneyStep(pointA, pointB, citizenID, travelMode, lineColor, endJourney);
                        lock (m_StepIndex)
                            m_StepIndex.Add(newindex, newStep);
                        m_HashToIdx.Add(hashname, newindex);
                        if (Singleton<JourneyVisualizer>.instance.LineMode && travelMode - 32 == Singleton<JourneyVisualizer>.instance.SelectedLine)
                            m_primarySteps.Add(newindex);
                        outlist.Add(newindex);
                    }
                }
                return outlist;
            }
        }


        // Reduce, the opposite of Augment, removes a citizen from the indexed JStep.  If the citizen count is reduced to zero, the step is removed from the Mgr
        // if the indexed JStep does not exist, does nothing (but error message to log)
        // check for the citizen not found in the JStep, error handling is in the jStep.ReduceStepData method (returns unchanged citizen count)
        //
        // code retained (commented out) for reference, but not used by JV (which never reduces, only adds or destroys completely)
        //
        //public void Reduce(ushort stepIndex, ushort citizenID)
        //{
        //    lock (mgrLock)
        //    {
        //        if (!m_StepIndex.TryGetValue(stepIndex, out JourneyStep jStep))
        //        {
        //            Debug.LogError("JV Error: Fatal lookup error for stepIndex " + stepIndex + " in JourneyStepMgr.Reduce");
        //            return;
        //        }
        //        string hashname = jStep.Hashname;
        //        if (jStep.ReduceStepData(citizenID) == 0)
        //        {
        //            m_StepIndex.Remove(stepIndex);
        //            m_HashToIdx.Remove(hashname);
        //            m_indexStack.Push(stepIndex);
        //        }
        //    }
        //}

        public void ResetMeshes()
        {
            lock (mgrLock)
            {
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                    {
                        Debug.Log("JV Error: null jStep within ReheatMeshes loop");
                        break;
                    }
                    jStep.SetRouteMeshes();
                }
            }
        }

        public void DrawTheMeshes(RenderManager.CameraInfo cameraInfo, int layerMask)
        {
            lock (mgrLock)
            {
                NetManager theNetManager = Singleton<NetManager>.instance;
                TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
                TransportManager theTransportManager = Singleton<TransportManager>.instance;
                Material xmaterial;
                Material xmaterial2;
                //bool xrequireSurfaceLine;
                int xlayer;
                //int xlayer2;
                TransportInfo transportInfo = Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Metro);
                if (transportInfo != null)
                {
                    xmaterial = transportInfo.m_lineMaterial2;
                    xmaterial2 = transportInfo.m_secondaryLineMaterial2;
                    // xrequireSurfaceLine = transportInfo.m_requireSurfaceLine;
                    xlayer = transportInfo.m_prefabDataLayer;
                    // xlayer2 = transportInfo.m_secondaryLayer;
                }
                else
                {
                    Debug.LogError("JV Error: Selected null material for rendering");
                    return;
                }
                Material material = (layerMask & 1 << xlayer) != 0 ? xmaterial : xmaterial2;
                // for debug
                int count = 0;
                int notrejected = 0;
                int uptofor = 0;
                int precamera = 0;
                int postcamera = 0;
                int drawmesh = 0;
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                        Debug.LogError("JV Error: null jStep in m_StepIndex dictionary");
                    jStep.DrawTheMeshes(cameraInfo, material);
                    count++;
                }
                //Debug.Log("JSM called DrawTheMeshes for " + count + " steps out of " + m_StepIndex.Count + ", JS notrejected: " + notrejected + " uptofor: " + uptofor + " precamera: " + precamera + " postcamera: " + postcamera + " drawmesh: " + drawmesh);
            }
        }

        // the next couple of functions are about the internal workings of maintaining the step list

        // Hashname uniquely identifies a step (this combination of content is what defines it as a separate step)
        // when creating the steps list, we look up Hashname in the mapping from hashnames to step numbers (m_HashToIdx) to see if we had it before
        // after that though, we only ever refer to steps by their number and the hashname mapping is not used again
        public string Hashname(Waypoint startpoint, Waypoint endpoint)
        {
            return startpoint.Segment + "+" + endpoint.Segment + "+" + startpoint.Offset + "+" + endpoint.Offset + "+" + startpoint.Lane + "+" + endpoint.Lane;
        }

        // Generate the next step number to use for StepsList (and to record the mapping from hashname to step number)
        // to save an unused index for re-use, you could Push it to m_indexStack  (but JV never does it)
        private ushort GetNewIndex()
        {
            ushort nextindex = m_indexStack.Pop();
            if (m_indexStack.Count == 0)
                m_indexStack.Push((ushort)(nextindex + 1));
            return nextindex;
        }

        // there is only ever one instance of a LaneLineSet, created for the purpose of splitting step data into lanes and lines, and retaining that info, for cycle viewing
        // its core is m_dataset, a double dictionary (indexed first by lane, then be line within lane) containg counts of cims in each combination
        // then m_laneIdx and m_lineIdx are created as mappings so you can refer to lanes 1, 2, 3 etc instead of needing to refer to actual lanes 321, 43, 67 etc
        public class LaneLineSet
        {
            private Dictionary<byte, Dictionary<ushort, HashSet<ushort>>> m_dataset;
            private List<byte> m_laneIDs;
            private List<List<ushort>> m_laneLineIDs;
            private int m_laneIdx;
            private int m_lineIdx;

            public LaneLineSet(HashSet<ushort> primarySteps, ushort selectedSegment)
            {
                m_dataset = new Dictionary<byte, Dictionary<ushort, HashSet<ushort>>>();
                foreach (ushort stepNum in primarySteps)
                {
                    JourneyStep jstep = Singleton<JourneyVisualizer>.instance.theStepManager.GetStep(stepNum);
                    byte thisLane = jstep.StartStep.Lane;
                    if (jstep.SubHeat > 0)
                    {
                        if (jstep.EndStep.Segment == selectedSegment)
                        {
                            thisLane = jstep.EndStep.Lane;
                        }
                        foreach (KeyValuePair<ushort, JourneyStep.CimStepInfo> pair in jstep.m_CimLines)
                        {
                            ushort line = (ushort)pair.Value.m_travelMode;
                            bool show = pair.Value.m_showCim;
                            ushort cim = pair.Key;
                            if (show)
                            {
                                if (!m_dataset.ContainsKey(thisLane))
                                    m_dataset.Add(thisLane, new Dictionary<ushort, HashSet<ushort>>());
                                if (!m_dataset[thisLane].ContainsKey(line))
                                    m_dataset[thisLane].Add(line, new HashSet<ushort> { cim });
                                else
                                    m_dataset[thisLane][line].Add(cim);
                            }
                        }
                    }
                }
                // setup indexers, this is a LIST filled from index 0 to end, use to convert (i, j) to (lane number, line number)
                m_laneIDs = new List<byte>();
                m_laneLineIDs = new List<List<ushort>>();
                foreach (byte lane in m_dataset.Keys)
                {
                    m_laneIDs.Add(lane);
                    m_laneLineIDs.Add(m_dataset[lane].Keys.ToList());
                }
                m_laneIdx = 0;
                m_lineIdx = -1;
            }

            // this is for cycle viewing of the lane-line dataset (a LaneLineSet object)
            // every call returns the next lane-line cims (as a hash set) ready to put into subselectedCims ready for ShowJourneys
            // at the end of the cycle it returns the whole of selectedCims (ie all lanes and lines combined)
            // (except if there is a secondary selection going, it reverts to the whole of the secondary selection)
            public HashSet<ushort> GetNextLaneLineCims(bool forwards = true, bool onlyPT = false)
            {
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                while (true)
                {
                    // hitting index pair 0 (lane) and -1 (lane-line) means show all (as of start of process)   
                    if (forwards)
                    {
                        if (++m_lineIdx > m_laneLineIDs[m_laneIdx].Count - 1)
                        {
                            m_lineIdx = 0;
                            if (++m_laneIdx > m_laneIDs.Count - 1)
                            {
                                m_laneIdx = 0;
                                m_lineIdx = -1;
                                return theJV.m_selectedCims;
                            }
                        }
                    }
                    else
                    {
                        if (--m_lineIdx < 0)
                        {
                            if (--m_laneIdx < 0)
                            {
                                m_laneIdx = m_laneIDs.Count - 1;
                                if (m_lineIdx == -2)
                                {
                                    m_lineIdx = m_laneLineIDs[m_laneIdx].Count;
                                }
                                else
                                {
                                    m_lineIdx = m_laneLineIDs[m_laneIdx].Count;
                                    return theJV.m_selectedCims;
                                }
                            }
                            else
                            {
                                m_lineIdx = m_laneLineIDs[m_laneIdx].Count - 1;
                            }
                        }
                    }
                    if (!onlyPT || (onlyPT && m_laneLineIDs[m_laneIdx][m_lineIdx] >= 32))
                    {
                        return m_dataset[m_laneIDs[m_laneIdx]][m_laneLineIDs[m_laneIdx][m_lineIdx]];
                    }
                }
            }
        }


    }
}
