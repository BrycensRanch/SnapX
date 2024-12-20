
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json.Serialization;
using SnapX.Core.Utils.Settings;

namespace SnapX.Core.Hotkey;

[JsonSerializable(typeof(HotkeysConfig))]
public class HotkeysConfig : SettingsBase<HotkeysConfig>
{
    public List<HotkeySettings> Hotkeys = HotkeyManager.GetDefaultHotkeyList();
}

