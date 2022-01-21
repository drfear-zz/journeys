using System;
using System.Collections.Generic;
using UnityEngine;

namespace Journeys
{
    public static class JVutils
    {
        public static Dictionary<int, Color> m_travelModeColors;
        static JVutils()
        {
            m_travelModeColors = new Dictionary<int, Color>();
            m_travelModeColors.Add(0, Color.white);  // representing serious error condition (segment info null)
            m_travelModeColors.Add(1, ColorUtils.Desaturate(Color.magenta, 0.3f));  // cars
            m_travelModeColors.Add(2, ColorUtils.Desaturate(Color.green, 0.3f));  // pedestrians
            m_travelModeColors.Add(3, Color.grey);  // bicycles
            m_travelModeColors.Add(4, Color.black);  // all other non-public-transport (mostly never selectable)
            m_travelModeColors.Add(5, Color.blue);  // generic for bus
            m_travelModeColors.Add(6, Color.green);  // metro
            m_travelModeColors.Add(7, Color.yellow);  // train
            m_travelModeColors.Add(8, Color.yellow);  // ship
            m_travelModeColors.Add(9, Color.yellow);  // airplane
            m_travelModeColors.Add(10, Color.yellow);  // taxi
            m_travelModeColors.Add(11, Color.magenta);  // tram
            m_travelModeColors.Add(12, Color.yellow);  // evac bus
            m_travelModeColors.Add(13, Color.yellow);  // monorail
            m_travelModeColors.Add(14, Color.yellow);  // cablecar
            m_travelModeColors.Add(15, Color.yellow);  // touristbus
            m_travelModeColors.Add(16, Color.yellow);  // hotairballon
            m_travelModeColors.Add(17, Color.yellow);  // post
            m_travelModeColors.Add(18, Color.yellow);  // trolleybus
            m_travelModeColors.Add(19, Color.yellow);  // fishing
            m_travelModeColors.Add(20, Color.yellow);  // helicopter
        }
    }
}