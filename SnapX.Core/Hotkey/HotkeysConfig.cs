
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json.Serialization;

namespace SnapX.Core.Hotkey;

public class HotkeysConfig
{
    public List<HotkeySettings> Hotkeys = HotkeyManager.GetDefaultHotkeyList();
}

