using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using UnityEngine;
using Journeys.RedirectionFramework.Attributes;
using System.Linq;
using System.Reflection;

// JourneySegmentData are the items in the Visualizer's m_gridData dictionary
// this functionality somewhat mimics the NetworkManager, but only holds segments used for (current) Journeys
// and also supplementing the data with a record of which citizen uses the segment, which part of it, and on what line

// as always, lineID 0 means not on public transport; this pseudo-id does not of itself differentiate eg pedestrian from private vehicle driver

namespace Journeys
{

    public struct JourneySegmentData
    {
        private Dictionary<ushort, HashSet<ushort>> m_segdata;

        ////private void AugmentLineCounts(ushort lineID)
        ////{
        ////    if (m_linecounts.TryGetValue(lineID, out ushort valnow))
        ////        m_linecounts[lineID] = ++valnow;
        ////    else
        ////        m_linecounts.Add(lineID, 1);
        ////}

        ////private void ReduceLineCounts(ushort lineID)
        ////{
        ////    if (m_linecounts.TryGetValue(lineID, out ushort valnow))
        ////    {
        ////        if (valnow == 1)
        ////            m_linecounts.Remove(lineID);
        ////        else
        ////            m_linecounts[lineID] = --valnow;
        ////    }
        ////}



        public JourneySegmentData(ushort citizenIDref, ushort lineID)
        {
            ////m_citizens = new HashSet<ushort> { citizenIDref };
            ////m_linecounts = new Dictionary<ushort, ushort>();
            ////m_linecounts.Add(lineID, 1);
            m_segdata = new Dictionary<ushort, HashSet<ushort>>();
            m_segdata.Add(citizenIDref, new HashSet<ushort> { lineID });
        }

        // returns false (and does not change anything) if the citizen-line combination is already recorded for the segment
        // such a return value is normally not an error, just for info
        public bool AugmentSegmentData(ushort citizenIDref, ushort lineID)
        {
            if (m_segdata.TryGetValue(citizenIDref, out HashSet<ushort> linedata))
            {
                return linedata.Add(lineID);
            }
            m_segdata.Add(citizenIDref, new HashSet<ushort> { lineID });
            return true;
            ////bool isnew = m_citizens.Add(citizenIDref);
            ////if (isnew)
            ////    AugmentLineCounts(lineID);
            ////return isnew;
        }

        // returns adjusted count of citizens on segment, so: 0 to indicate reduced to empty count (ie object should be removed from JV.m_gridData)
        // NOTE it is not (necessarily) an error to attempt to remove a citizen-line pair that does not exist, because this pairing can happen
        // more than once on a segment (eg tram at stop, same tram at end of segment), and the pairing may have been removed by the first call to this function
        public int ReduceSegmentData(ushort citizenIDref, ushort lineID)
        {
            if (m_segdata.TryGetValue(citizenIDref, out HashSet<ushort> linedata))
            {
                linedata.Remove(lineID);
                if (linedata.Count == 0)
                    m_segdata.Remove(citizenIDref);
            }
            return m_segdata.Count;
            ////if (m_citizens.Remove(citizenIDref))
            ////    ReduceLineCounts(lineID);
            ////return m_citizens.Count;
        }

        public Color getHeat(float denominator = 0)
        {
            if (denominator == 0)
                denominator = Singleton<JourneyVisualizer>.instance.m_journeysCount;
            return Color.HSVToRGB(H: (1f - (m_segdata.Count / denominator)) / 2, S: 0.8f, V: 0.8f);
        }

        ////public void Dprint()
        ////{
        ////    string outstring = "\njsegdata_citizens: ";
        ////    foreach (ushort ctz in m_citizens)
        ////        outstring = outstring + ctz + " ";
        ////    outstring = outstring + "\njsegdata_lines: ";
        ////    foreach (var pair in m_linecounts)
        ////        outstring = outstring + "[line " + pair.Key + "]:" + pair.Value + " ";
        ////    Debug.Log(outstring);
        ////}
    }
}

// note about implementation:
// very tempting to store the render meshes here also, so just once for every segment used, rather than repeat for every subjourney that passes through
// this would in fact work only for segments that were part of identical journeys (different journeys have different arrows and potentially
// different connections to different next segments)
// Besides which - and this is overriding - the correspondence between steps and meshes is not one-one!  A subjourney has a mesh that may be (often is)
// a different length to the number of steps.