using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Journeys
{

    // A Waypoint is a network-based location (ie based on segment and position within the segment, rather than being
    // a vector gridReference. A JourneyStep is the journey from one Waypoint to another (ie has a start and end Waypoint).
    // It may be noted that a Waypoint is identical to a PathUnit.Position, except for the crucial addition of travelMode

    public class Waypoint
    {
        public ushort Segment { get; private set; }
        public byte Offset { get; set; }
        public byte Lane { get; }
        public int TravelMode { get; set; }


        // for debugging
        public void Dprint()
        {
            Debug.Log("\nsegment: " + Segment + ", offset: " + Offset + ", lane: " + Lane + ", travelMode: " + TravelMode);
        }

        public Waypoint(PathUnit.Position pathPosition, int travelMode = -1)
        {
            Segment = pathPosition.m_segment;
            Offset = pathPosition.m_offset;
            Lane = pathPosition.m_lane;
            TravelMode = travelMode;                         // -1 indicates not set (yet). TravelMode 0 indicates "unresolvable error" in setting, such as deficient segmentInfo
        }

        public Waypoint(ushort seg, byte lane, byte offset, int travelmode)
        {
            Segment = seg;
            Offset = offset;
            Lane = lane;
            TravelMode = travelmode;
        }

        // rationalize is to "correct" for transport stops sometimes not being at exactly 128 for all lines. It only affects the position of transport stops.
        public void Rationalize()
        {
            if (TravelMode > 31)
                if (Offset != 0 && Offset != 255)
                    Offset = 128;
        }
    }
}
