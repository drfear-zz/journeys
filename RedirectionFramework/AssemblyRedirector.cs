﻿using System;
using System.Linq;
using System.Reflection;
using Journeys.RedirectionFramework.Extensions;
using Journeys.RedirectionFramework.Attributes;

namespace Journeys.RedirectionFramework
{

    public class AssemblyRedirector
    {
        private static Type[] _types;

        public static void Deploy()
        {
            _types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(TargetTypeAttribute), false).Length > 0).ToArray();
            foreach (var type in _types)
            {
                type.Redirect();
            }
        }

        public static void Revert()
        {
            if (_types == null)
            {
                return;
            }
            foreach (var type in _types)
            {
                type.Revert();
            }
            _types = null;
        }

    }


}
