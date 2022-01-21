using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
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
using System.Collections;

namespace Journeys
{
	public class JourneyStepMgr : MonoBehaviour
	{
        [NonSerialized]
        private Dictionary<ushort, JourneyStep> m_ushortSteps;
        [NonSerialized]
        private Dictionary<string, ushort> m_HashToIdx;
        public static JourneyStepMgr instance;
        private static Stack<ushort> m_indexStack;

        public void Awake()
        {
            instance = this;
            Debug.Log("JourneyStepMgr awake");
        }

        public void Init()
        {
            m_ushortSteps = new Dictionary<ushort, JourneyStep>();
            m_HashToIdx = new Dictionary<string, ushort>();
            m_indexStack = new Stack<ushort>();
            m_indexStack.Push(1);
            Debug.Log("JourneyStepMgr Init has been run");
        }

        public JourneyStep GetStep(ushort index) => m_ushortSteps[index];

        public ushort Augment(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney = false, bool show = true)
        {
            string hashname = Hashname(route[0], route[route.Count - 1]);
            if (m_HashToIdx.TryGetValue(hashname, out ushort uindex))
            {
                Debug.Log("Augment for existing step, cim " + citizenID + ", stepIndex " + uindex + ", hashname " + hashname);
                m_ushortSteps[uindex].AugmentStepData(citizenID, lineColorPair, show);
                return uindex;
            }
            else
            {
                ushort newindex = GetNewIndex();
                Debug.Log("Augment for new step, cim " + citizenID + ", newIndex " + newindex + ", hashname " + hashname);
                JourneyStep newStep = new JourneyStep(route, citizenID, lineColorPair, endJourney, show);
                m_ushortSteps.Add(newindex, newStep);
                m_HashToIdx.Add(hashname, newindex);
                return newindex;
            }
        }

        public void Reduce(ushort stepIndex, ushort citizenID)
        {
            if (!m_ushortSteps.TryGetValue(stepIndex, out JourneyStep jStep))
                Debug.LogError("Fatal lookup error for stepIndex " + stepIndex + " in JourneyStepMgr.Reduce");
            string hashname = jStep.Hashname;
            Debug.Log("Reduce for cim " + citizenID + ", stepIndex " + stepIndex + ", hashname " + hashname);
            if (jStep.ReduceStepData(citizenID) == 0)
            {
                m_ushortSteps.Remove(stepIndex);
                m_HashToIdx.Remove(hashname);
                m_indexStack.Push(stepIndex);
            }
        }

        public void DestroyAll()
        {
            foreach (JourneyStep jStep in m_ushortSteps.Values)
            {
                jStep.KillLineMeshes();
                jStep.KillRouteMeshes();
            }
            m_ushortSteps = null;
            m_HashToIdx = null;
            m_indexStack = null;
        }

        public string Hashname(Waypoint startpoint, Waypoint endpoint)
        {
            return startpoint.Segment + "+" + endpoint.Segment + "+" + startpoint.Offset + "+" + endpoint.Offset + "+" + startpoint.Lane + "+" + endpoint.Lane;
        }

        // to save an index for re-use, just Push it to m_indexStack
        private ushort GetNewIndex()
        {
            string str = "";
            foreach (ushort i in m_indexStack)
            {
                str = str + i + ", ";
            }
            Debug.Log("index stack on call for new: " + str);
            ushort nextindex = m_indexStack.Pop();
            if (m_indexStack.Count == 0)
                m_indexStack.Push((ushort)(nextindex + 1));
            return nextindex;
        }
    }
}
