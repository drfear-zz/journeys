using ColossalFramework;
using System;
using System.Collections.Generic;

namespace Journeys
{

	// a Segway is esstentially an extended Segment.Info. There is a Segway for every segment that has any journey along any part of it,
	// held in Dictionary JV.m_segways, indexed by segment number
	// reflecting the two distinct types of segments (normal aka land, and public transport), Segways carry a flag .IsTransport
	// They also carry a public flag .TransportSegwaysSet, if true this means the (land) segment has already been recorded in the dictionary as part of a PT route
	// (strictly speaking this isn't truly a part of a Segway, but it is convenient to avoid an extra dictionary to record this event)

	// Using JV.segways dictionary speeds up (dramatically) picking out the cims (and hence their journeys) for segment-type selections
	//  at a trade-off that the one-time build of the dictionary is slow, the equivalent of setting up for Show all journeys, even if you don't want to do that


	public class SegWay
	{
		// first define members that determine attributes for transport segments (if this is a transport segment)
		public bool IsTransport { get; }       // meaning this is a transport segment, not a drawable map segment
		public ushort m_line;                  // set if this is a transport segment. Line number can be zero (planes, intercity trains), so this is not of itself a test for transport segment
		public List<Waypoint> m_landroute;     // this is only set for a segment that is a PT segment
		public bool TransportSegWaysSet { get; set; }       // when true this records that the landroute Segways have been used before, so you don't need to test for new segways, they ARE in the dictionary

		public HashSet<ushort> m_cims;        // an overall list for quick by-segment selections

		public SegWay(ushort segment, bool knownLand = false)
		{
			TransportSegWaysSet = false;
			if (!knownLand)
			{
				NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[segment];
				uint landpathID = netSegment.m_path;
				if (landpathID != 0)
				{
					m_landroute = JVutils.PathToWaypoints(landpathID);
					m_line = Singleton<NetManager>.instance.m_nodes.m_buffer[netSegment.m_startNode].m_transportLine;
					IsTransport = true;
					return;
				}
			}
			m_line = 0;
			m_cims = new HashSet<ushort>();
			IsTransport = false;
		}
		// unsafe will cause a crash if called on a transport segment (because then m_cims is unitialized). Even if it did not crash everything would go pear shaped.
		public void AddCimUnsafe(ushort cim)
		{
			m_cims.Add(cim);
		}
		public bool AddCimSafe(ushort cim)
		{
			if (!IsTransport)
				m_cims.Add(cim);
			return !IsTransport;
		}
	}


}