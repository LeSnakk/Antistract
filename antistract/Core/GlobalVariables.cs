﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antistract.Core
{
    public class GlobalVariables
    {
        public static List<String> PlanNames { get; set; } = new List<string>();

        public static bool OnlyPausing;

        public static bool BrowserClose;

        public static bool TimerRunning;

        public static TimerWindow timerWindow;

        public static bool CheckBrowser = false;

        public static bool CheckPrograms = false;
    }
}
