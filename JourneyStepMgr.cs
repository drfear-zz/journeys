using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        public int StepCount => m_StepIndex.Count;

        private readonly object mgrLock = new object();

        public JourneyStepMgr()
        {
            m_StepIndex = new Dictionary<ushort, JourneyStep>();
            m_HashToIdx = new Dictionary<string, ushort>();
            m_indexStack = new Stack<ushort>();
            m_indexStack.Push(1);
            Debug.Log("JV: JourneyStepMgr constructor has been run");
        }

        public void WipeSlate()
        {
            lock (mgrLock)
            {
                Debug.Log("JV: entered StepMgr.WipeSlate");
                foreach (JourneyStep jStep in m_StepIndex.Values)
                    jStep.KillMeshes();
                m_StepIndex.Clear();
                m_HashToIdx.Clear();
                m_indexStack.Clear();
                m_indexStack.Push(1);
                Debug.Log("JV: finished StepMgr.WipeSlate");
            }
        }

        // Augment will either add a new JStep for a new step, for a given citizen journey, or increment an existing step
        // it returns the ushort index of the new (or pre-existing) step, which will not be zero
        public ushort Augment(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney = false, bool show = true)
        {
            lock (mgrLock)
            {
                string hashname = Hashname(route[0], route[route.Count - 1]);
                if (m_HashToIdx.TryGetValue(hashname, out ushort uindex))
                {
                    //Debug.Log("Mgr Augment for existing step, cim " + citizenID + ", stepIndex " + uindex + ", hashname " + hashname);
                    m_StepIndex[uindex].AugmentStepData(citizenID, lineColorPair, show);
                    return uindex;
                }
                else
                {
                    ushort newindex = GetNewIndex();
                    //Debug.Log("Mgr Augment for new step, cim " + citizenID + ", newIndex " + newindex + ", hashname " + hashname);
                    JourneyStep newStep = new JourneyStep(route, citizenID, lineColorPair, endJourney, show);
                    m_StepIndex.Add(newindex, newStep);
                    m_HashToIdx.Add(hashname, newindex);
                    return newindex;
                }
            }
        }

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
                //Debug.Log("Reduce for cim " + citizenID + ", stepIndex " + stepIndex + ", hashname " + hashname);
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
                //Debug.Log("JV: within lock in CalculateMeshes, num steps is " + m_StepIndex.Count);
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                        Debug.Log("JV Error: null jStep within CalculateMeshes loop");
                    jStep.SetRouteMeshes();
                }
                Debug.Log("JV: CalculateMeshes has called SetRouteMesh for all " + m_StepIndex.Count + " jSteps");
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
                        return;
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
                //Debug.Log("JV: m_stepIndex.Count is " + m_StepIndex.Count);
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    if (jStep == null)
                        Debug.LogError("JV: null jStep in m_StepIndex dictionary");
                    jStep.DrawTheMeshes(cameraInfo, material);
                }
            }
        }

        public void HitSegmentLane(ushort segmentID, byte lane)
        {
            lock (mgrLock)
            {
                HashSet<ushort> hitlist = new HashSet<ushort>();
                foreach (JourneyStep jStep in m_StepIndex.Values)
                {
                    List<ushort> outlist = jStep.HitSegmentLane(segmentID, lane);
                    if (outlist != null) {
                        foreach (ushort cim in outlist)
                            hitlist.Add(cim);
                    }
                }
                // I might at some point break this journey flagging out into a private (because would have to be unlocked) method
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                foreach (ushort cim in hitlist)
                {
                    Journey journey = theJV.m_journeys[cim];
                    foreach (ushort stepIdx in journey.m_steps)
                    {
                        m_StepIndex[stepIdx].ShowCitizen(cim);
                    }
                    theJV.m_selectedJourneysCount = hitlist.Count;
                }
            }
        }

        public void FromToHere(int ftflag, ushort targetSegment)
        {
            Debug.Log("call FromToHere with ftflag " + ftflag + " and target segment " + targetSegment);
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (ftflag == 0)
            {
                foreach (Journey journey in theJV.m_journeys.Values)
                {
                    ushort cim = journey.m_id;
                    foreach (ushort stepIdx in journey.m_steps)
                        m_StepIndex[stepIdx].ShowCitizen(cim);
                }
            }
            else
            {
                if (ftflag == 1)
                {
                    foreach (Journey journey in theJV.m_journeys.Values)
                    {
                        ushort cim = journey.m_id;
                        bool after = false;
                        foreach (ushort stepIdx in journey.m_steps)
                        {
                            bool target = m_StepIndex[stepIdx].StartStep.Segment == targetSegment;
                            if (after || target)
                            {
                                m_StepIndex[stepIdx].ShowCitizen(cim);
                                if (target)
                                    after = true;
                            }
                            else
                                m_StepIndex[stepIdx].HideCitizen(cim);
                        }
                    }
                }
                else
                {
                    foreach (Journey journey in theJV.m_journeys.Values)
                    {
                        ushort cim = journey.m_id;
                        bool before = true;
                        foreach (ushort stepIdx in journey.m_steps)
                        {
                            bool target = m_StepIndex[stepIdx].EndStep.Segment == targetSegment;
                            if (before || target)
                            {
                                m_StepIndex[stepIdx].ShowCitizen(cim);
                                if (target)
                                    before = false;
                            }
                            else
                                m_StepIndex[stepIdx].HideCitizen(cim);
                        }
                    }
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
    }
}
