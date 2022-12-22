using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Journeys
{

    //[TargetType(typeof(PathVisualizer))]

    public class JourneyStepMgr
	{
        [NonSerialized]
        private Dictionary<ushort, JourneyStep> m_StepIndex;
        [NonSerialized]
        private Dictionary<string, ushort> m_HashToIdx;
        //public static JourneyStepMgr instance;
        private Stack<ushort> m_indexStack;
        private HashSet<ushort> m_targetSteps;
        private HashSet<ushort> m_targetSteps2;


        public int StepCount => m_StepIndex.Count;

        private readonly object mgrLock = new object();

        public JourneyStepMgr()
        {
            m_StepIndex = new Dictionary<ushort, JourneyStep>();
            m_HashToIdx = new Dictionary<string, ushort>();
            m_indexStack = new Stack<ushort>();
            m_indexStack.Push(1);
            m_targetSteps = new HashSet<ushort>();
        }

        public void WipeSlate()
        {
            lock (mgrLock)
            {
                lock (m_StepIndex)
                {
                    foreach (JourneyStep jStep in m_StepIndex.Values)
                        jStep.KillMeshes();
                    m_StepIndex.Clear();
                    m_HashToIdx.Clear();
                    m_indexStack.Clear();
                    m_indexStack.Push(1);
                    m_targetSteps.Clear();
                }
            }
        }

        public void HideAllCims()
        {
            lock (mgrLock)
            {
                foreach (JourneyStep jstep in m_StepIndex.Values)
                {
                    jstep.HideAllCims(dolock: true);
                }
            }
        }

        public HashSet<ushort> GetTarget2Cims(ushort segment2)
        {
            m_targetSteps2 = new HashSet<ushort>();
            HashSet<ushort> cimHash = new HashSet<ushort>();
            foreach (KeyValuePair<ushort, JourneyStep> pair in m_StepIndex)
            {
                JourneyStep jstep = pair.Value;
                ushort idx = pair.Key;
                if (jstep.StartStep.Segment == segment2 || jstep.EndStep.Segment == segment2)
                {
                    m_targetSteps2.Add(idx);
                    foreach (ushort cim in jstep.GetCimsDict().Keys)
                        cimHash.Add(cim);
                }
            }
            return cimHash;
        }



        public HashSet<ushort> GetLaneCims(ushort segmentID, byte lane)
        {
            HashSet<ushort> laneCims = new HashSet<ushort>();
            lock (mgrLock)
            {
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    List<ushort> outlist = jStep.HitSegmentLane(segmentID, lane);
                    if (outlist != null)
                    {
                        foreach (ushort cim in outlist)
                            laneCims.Add(cim);
                    }
                }
            }
            return laneCims;
        }


        // Thinking ahea to GUI usage, this collection allows for random access of a line on a lane, or
        // you can scroll through the contents with .Next
        public class LaneLineSet
        {
            private Dictionary<byte, Dictionary<ushort, HashSet<ushort>>> m_dataset;
            private List<byte> m_laneIDs;
            private List<List<ushort>> m_laneLineIDs;
            private int m_laneIdx;
            private int m_lineIdx;

            public LaneLineSet(List<ushort> targets, ushort selectedSegment, NetInfo selectedSegmentInfo)
            {
                m_dataset = new Dictionary<byte, Dictionary<ushort, HashSet<ushort>>>();
                //var lineDic = new Dictionary<ushort, HashSet<ushort>>();
                foreach (ushort tgt in targets)
                {
                    // only include steps that TRAVERSE at least part of the target segment (should do this globally in the first place really)
                    JourneyStep jstep = Singleton<JourneyVisualizer>.instance.theStepManager.GetStep(tgt);
                    //bool hit = true;
                    //byte thisLane = jstep.EndStep.Lane;

                    //if (jstep.EndStep.Segment == selectedSegment) {
                    //    NetInfo.Direction direction = selectedSegmentInfo.m_lanes[jstep.EndStep.Lane].m_direction;
                    //    if ( (jstep.EndStep.Offset == 0 && direction == NetInfo.Direction.Forward) ||
                    //            (jstep.EndStep.Offset == 255 && direction == NetInfo.Direction.Backward))
                    //    {
                    //        hit = false;
                    //    }
                    //}
                    //else if (jstep.StartStep.Segment == selectedSegment)
                    //{
                    //    thisLane = jstep.StartStep.Lane;
                    //    NetInfo.Direction direction = selectedSegmentInfo.m_lanes[jstep.StartStep.Lane].m_direction;
                    //    if ((jstep.StartStep.Offset == 0 && direction == NetInfo.Direction.Backward) ||
                    //            (jstep.StartStep.Offset == 255 && direction == NetInfo.Direction.Forward))
                    //    {
                    //        hit = false;
                    //    }
                    //}
                    //if (hit)
                    //{
                    byte thisLane = jstep.StartStep.Lane;
                    if (jstep.SubHeat > 0)
                    {
                        if (jstep.EndStep.Segment == selectedSegment)
                        {
                            thisLane = jstep.EndStep.Lane;
                        }
                        foreach (KeyValuePair<ushort, JourneyStep.CimStepInfo> pair in jstep.GetCimsDict())
                        {
                            ushort line = (ushort)pair.Value.m_travelMode;
                            bool show = pair.Value.m_showCim;
                            ushort cim = pair.Key;
                            //if (!lineDic.ContainsKey(line))
                            //    lineDic.Add(line, new HashSet<ushort> { cim });
                            //else
                            //    lineDic[line].Add(cim);
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
                    //}
                }
                // setup indexers, this is a LIST filled from index 0 to end, use to convert (i, j) to (lane number, line number)
                m_laneIDs = new List<byte>();
                m_laneLineIDs = new List<List<ushort>>();
                foreach (byte lane in m_dataset.Keys)
                {
                    m_laneIDs.Add(lane);
                    m_laneLineIDs.Add(m_dataset[lane].Keys.ToList());
                }
                //m_laneIdx = m_laneIDs.Count - 1;
                //m_lineIdx = m_laneLineIDs[m_laneIdx].Count - 1;  // set to the last item in the last lanes list, so GetNextLane will start from (0,0) (effective)
                // change policy, because the hashset is arbitrary sequence, just set to 0,-1 will become 0,0
                m_laneIdx = 0;
                m_lineIdx = -1; 
            }

            public HashSet<ushort> GetNextLaneLineCims(bool forwards = true, bool onlyPT = false)
            {
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                while (true)
                {
                    // hitting index pair 0 (lane) and -1 (lane-line) means show all        
                    if (forwards)
                    {
                        if (++m_lineIdx > m_laneLineIDs[m_laneIdx].Count - 1)
                        {
                            m_lineIdx = 0;
                            if (++m_laneIdx > m_laneIDs.Count - 1)
                            {
                                m_laneIdx = 0;
                                m_lineIdx = -1;
                                if (theJV.SelectedSegment2 == 0)
                                    return theJV.m_selectedCims;
                                else
                                    return theJV.m_subselectedCims2;
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
                                    if (theJV.SelectedSegment2 == 0)
                                        return theJV.m_selectedCims;
                                    else
                                        return theJV.m_subselectedCims2;
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
                    //Debug.Log("m_LaneIdx " + m_laneIdx + "; m_lineIdx " + m_lineIdx);
                    //Debug.Log("m_laneIDs.Count " + m_laneIDs.Count + "; m:lanelineIDs.Count " + m_laneLineIDs.Count);
                }
            }
        }

        public List<ushort> GetTargetSteps()
        {
            lock (mgrLock)
            {
                return m_targetSteps.ToList();
            }
        }
        public List<ushort> GetTarget2Steps()
        {
            lock (mgrLock)
            {
                return m_targetSteps2.ToList();
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
        // it returns the ushort index of the new (or pre-existing) step, which will not be zero
        //public ushort Augment(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney = false, bool show = true)
        //{
        //    lock (mgrLock)
        //    {
        //        string hashname = Hashname(route[0], route[route.Count - 1]);
        //        if (m_HashToIdx.TryGetValue(hashname, out ushort uindex))
        //        {
        //            //Debug.Log("Mgr Augment for existing step, cim " + citizenID + ", stepIndex " + uindex + ", hashname " + hashname);
        //            m_StepIndex[uindex].AugmentStepData(citizenID, lineColorPair, show);
        //            //Debug.Log("added to existing step " + uindex + ")");
        //            return uindex;
        //        }
        //        else
        //        {
        //            ushort newindex = GetNewIndex();
        //            //Debug.Log("Mgr Augment for new step, cim " + citizenID + ", newIndex " + newindex + ", hashname " + hashname);
        //            JourneyStep newStep = new JourneyStep(route, citizenID, lineColorPair, endJourney, show);
        //            m_StepIndex.Add(newindex, newStep);
        //            m_HashToIdx.Add(hashname, newindex);
        //            //Debug.Log("added to new step " + newindex + ")");
        //            // now add to targetSteps if appropriate
        //            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
        //            foreach (Waypoint waypoint in route)
        //            {
        //                InstanceID newID = InstanceID.Empty;
        //                newID.NetSegment = waypoint.Segment;
        //                if (theJV.m_targets.Contains(newID))
        //                {
        //                    m_targetSteps.Add(newindex);
        //                    break;
        //                }
        //            }
        //            return newindex;
        //        }
        //    }
        //}
        public List<ushort> Augment(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney = false, bool show = true, 
            bool skipCornerCheck = false)
        {
            lock (mgrLock)
            {
                //(Debug.Log("Augment for cim " + citizenID + " skipCornerCheck " + skipCornerCheck);
                if (route.Count < 2)
                    return null;
                List<ushort> outlist = new List<ushort>();
                Waypoint pointA;
                Waypoint pointB;
                List<Waypoint> subroute;
                for (int idx = 0; idx < route.Count - 1; idx++)
                {
                    pointA = route[idx];
                    pointA.Rationalize(lineColorPair);
                    pointB = route[idx + 1];
                    pointB.Rationalize(lineColorPair);
                    // rationalize PT offsets to 0, 128 and 255
                    // make endJourney apply only to the last step in the route
                    bool lastStepEnd = idx == route.Count - 2 && endJourney;
                    if (!skipCornerCheck)
                    //if (!skipCornerCheck && lineColorPair.m_travelmode > 31)
                    {
                        if (JVutils.TurnsCorner(pointA, pointB, out Waypoint pointBprime))
                        {
                            if (JVutils.PassesStop(pointBprime, pointB, out Waypoint pointStop))
                                subroute = new List<Waypoint> { pointA, pointBprime, pointStop, pointB };
                            else
                                subroute = new List<Waypoint> { pointA, pointBprime, pointB };
                            outlist.AddRange(Augment(subroute, citizenID, lineColorPair, lastStepEnd, show, skipCornerCheck: true));
                        }
                        else
                        {
                            if (JVutils.PassesStop(pointA, pointB, out Waypoint pointStop))
                                subroute = new List<Waypoint> { pointA, pointStop, pointB };
                            else
                                subroute = new List<Waypoint> { pointA, pointB };
                            outlist.AddRange(Augment(subroute, citizenID, lineColorPair, lastStepEnd, show, skipCornerCheck: true));
                        }
                    }
                    else
                    {
                        string hashname = Hashname(pointA, pointB);
                        if (m_HashToIdx.TryGetValue(hashname, out ushort uindex))
                        {
                            m_StepIndex[uindex].AugmentStepData(citizenID, lineColorPair, show);
                            outlist.Add(uindex);
                        }
                        else
                        {
                            ushort newindex = GetNewIndex();
                            subroute = new List<Waypoint> { pointA, pointB };
                            JourneyStep newStep = new JourneyStep(subroute, citizenID, lineColorPair, lastStepEnd, show);
                            lock (m_StepIndex)
                                m_StepIndex.Add(newindex, newStep);
                            m_HashToIdx.Add(hashname, newindex);
                            if (!Singleton<JourneyVisualizer>.instance.LineMode)
                            {
                                foreach (Waypoint waypoint in subroute)
                                {
                                    InstanceID newID = InstanceID.Empty;
                                    newID.NetSegment = waypoint.Segment;
                                    if (Singleton<JourneyVisualizer>.instance.m_targets.Contains(newID))
                                    {
                                        m_targetSteps.Add(newindex);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (lineColorPair.m_travelmode - 32 == Singleton<JourneyVisualizer>.instance.SelectedLine)
                                    m_targetSteps.Add(newindex);
                            }
                            outlist.Add(newindex);
                        }
                    }
                }
                return outlist;
            }
        }

        //public void ReStep()
        //{
        //    Dictionary<string, List<ushort>> linkedSteps = new Dictionary<string, List<ushort>>();
        //    Dictionary<ushort, List<ushort>> stepMap = new Dictionary<ushort, List<ushort>>();
        //    string idxStr;
        //    foreach (KeyValuePair<ushort, JourneyStep> pair in m_StepIndex)
        //    {
        //        foreach (Waypoint wpoint in pair.Value.m_route)
        //        {
        //            idxStr = wpoint.Segment + "+" + wpoint.Lane;
        //            if (linkedSteps.TryGetValue(idxStr, out List<ushort> stepIdxList))
        //            {
        //                stepIdxList.Add(pair.Key);
        //                linkedSteps[idxStr] = stepIdxList;
        //            }
        //            else
        //            {
        //                linkedSteps.Add(idxStr, new List<ushort> { pair.Key });
        //            }
        //        }
        //    }
        //    foreach (KeyValuePair<string, List<ushort>> pair in linkedSteps)
        //    {
        //        idxStr = pair.Key;
        //        string[] idxStrSplit = idxStr.Split('\u002B');
        //        ushort idxSeg = (ushort)int.Parse(idxStrSplit[0]);
        //        byte idxLane = (byte)int.Parse(idxStrSplit[1]);
        //        List<ushort> stepIdxList = pair.Value;
        //        if (stepIdxList.Count == 1)
        //            continue;
        //        bool firstStep = true;
        //        List<ushort> newStepIdxList = new List<ushort>();
        //        List<JourneyStep> splitSteps = new List<JourneyStep>();
        //        foreach (ushort oldStepIdx in stepIdxList)
        //        {
        //            if (firstStep)
        //            {
        //                // this step may have already been split, check in the map
        //                int newStepIndex = -1;
        //                int cutpoint = -1;
        //                if (stepMap.TryGetValue(oldStepIdx, out newStepIdxList))
        //                {
        //                    for (int idx = 0; idx < newStepIdxList.Count; idx++)
        //                    {
        //                        JourneyStep step = m_StepIndex[newStepIdxList[idx]];
        //                        int cutpoint = -1;
        //                        for (int jdx = step.m_route.Count - 1; jdx >= 0; jdx--)  // go backwards to pick up the LAST occurence of index seg
        //                        {
        //                            Waypoint wpoint = step.m_route[jdx];
        //                            if (wpoint.Segment == idxSeg && wpoint.Lane == idxLane)
        //                            {
        //                                cutpoint = jdx;
        //                                break;
        //                            }
        //                        }
        //                        if (cutpoint >= 0)
        //                        {
        //                            newStepIndex = idx;
        //                            break;
        //                        }
        //                    }
        //                    if (newStepIndex == -1)
        //                        Debug.Log("Everything has gone horribly wrong.  Give it it all up");

        //                    // we know which one to maybe split now, go do that
        //                    List<ushort> newSplits = SplitStep(newStepIdxList[newStepIndex], cutpoint);


        //                }

        //                // map this step to any replacing it (if none, use as is)

        //                // for multi-segment Steps, split into 3 (potentially)
        //                newSteps.Add(m_StepIndex[stepIdx]);
        //                firstStep = false;
        //            }
        //            else
        //            {
        //                JourneyStep nextStep = m_StepIndex[stepIdx];
        //                if (nextStep.StartStep.Segment != newSteps[0].StartStep.Segment)
        //                {
        //                    // find the common segment, split both before and after, merge the middle like normal
        //                }
        //                if (nextStep.StartStep.Offset < newSteps[0].StartStep.Offset)
        //                {

        //                }
        //            }
        //            ushort limits = Starter stop

        //        }

        //    }
        //}

        // Reduce removes a citizen from the indexed JStep.  If the citizen count is reduced to zero, the step is removed from the Mgr
        // if the indexed JStep does not exisit, does nothing (but error message to log)
        // check for the citizen not found in the JStep, error handling is in the jStep.ReduceStepData method (returns unchanged citizen count)
        public void Reduce(ushort stepIndex, ushort citizenID)
        {
            lock (mgrLock)
            {
                if (!m_StepIndex.TryGetValue(stepIndex, out JourneyStep jStep))
                {
                    Debug.LogError("JV Error: Fatal lookup error for stepIndex " + stepIndex + " in JourneyStepMgr.Reduce");
                    return;
                }
                string hashname = jStep.Hashname;
                if (jStep.ReduceStepData(citizenID) == 0)
                {
                    m_StepIndex.Remove(stepIndex);
                    m_HashToIdx.Remove(hashname);
                    m_indexStack.Push(stepIndex);
                }
            }
        }

        public void CalculateMeshes()
        {
            lock (mgrLock)
            {
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                    {
                        Debug.Log("JV Error: null jStep within CalculateMeshes loop");
                        break;
                    }
                    jStep.SetRouteMeshes();
                }
            }
        }

        public void ReheatMeshes()
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
                    jStep.SetRouteMeshes(forceReheat: true);
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
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                        Debug.LogError("JV: null jStep in m_StepIndex dictionary");
                    jStep.DrawTheMeshes(cameraInfo, material);
                }
            }
        }

        public void DestroyAll()
        {
            lock (mgrLock)
            {
                foreach (JourneyStep jStep in m_StepIndex.Values)
                    jStep.KillMeshes();
                m_StepIndex = null;
                m_HashToIdx = null;
                m_indexStack = null;
            }
        }

        public string Hashname(Waypoint startpoint, Waypoint endpoint)
        {
            return startpoint.Segment + "+" + endpoint.Segment + "+" + startpoint.Offset + "+" + endpoint.Offset + "+" + startpoint.Lane + "+" + endpoint.Lane;
        }

        // to save an index for re-use, just Push it to m_indexStack
        private ushort GetNewIndex()
        {
            ushort nextindex = m_indexStack.Pop();
            if (m_indexStack.Count == 0)
                m_indexStack.Push((ushort)(nextindex + 1));
            return nextindex;
        }

        public void LogSteps()
        {
            foreach (KeyValuePair<ushort, JourneyStep> pair in m_StepIndex)
                pair.Value.DumpStep(pair.Key);
        }
    }
}
