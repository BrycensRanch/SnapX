
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.ScreenCaptureLib
{
    public class ScrollingCaptureOptions
    {
        public int StartDelay { get; set; } = 300;
        public bool AutoScrollTop { get; set; } = false;
        public int ScrollDelay { get; set; } = 300;
        public int ScrollAmount { get; set; } = 2;
        public bool AutoIgnoreBottomEdge { get; set; } = true;
        public bool AutoUpload { get; set; } = false;
        public bool ShowRegion { get; set; } = true;
    }
}