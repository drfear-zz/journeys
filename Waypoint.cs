using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Journeys
{

	// A Waypoint is a network-based location (ie based on segment and position within the segment, rather than being
	// a vector gridReference. A JourneyStep is the journey from one Waypoint to another (ie has a start and end Waypoint).
	// A Waypoint is basically like a pathmanager PathUnit.Position, except for the rationalization of Offset

	public class Waypoint
	{
		public ushort Segment { get; private set; }
		public byte Offset { get; set; }
		public byte Lane { get; }


		//// for debugging
		//public void Dprint()
		//{
		//	Debug.Log("\nsegment: " + Segment + ", offset: " + Offset + ", lane: " + Lane + ", travelMode: " + TravelMode);
		//}

		public Waypoint(PathUnit.Position pathPosition)
		{
			Segment = pathPosition.m_segment;
			Lane = pathPosition.m_lane;
			bool StartAt0 = Singleton<NetManager>.instance.m_segments.m_buffer[Segment].Info.m_lanes[Lane].m_position == 0;
			Offset = pathPosition.m_offset;
			if (Offset != 255 && Offset != 0)
			{
				if (StartAt0)
				{
					if (Offset > 128)
						Offset = 255;
					else
						Offset = 128;
				}
				else
				{
					if (Offset < 128)
						Offset = 0;
					else
						Offset = 128;
				}
			}
		}

		public Waypoint(ushort seg, byte lane, byte offset)
		{
			Segment = seg;
			Offset = offset;
			Lane = lane;
		}
	}
}
