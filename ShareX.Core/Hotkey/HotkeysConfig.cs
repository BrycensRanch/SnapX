
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json.Serialization;
using ShareX.Core.Utils.Settings;

namespace ShareX.Core.Hotkey;

[JsonSerializable(typeof(HotkeysConfig))]
public class HotkeysConfig : SettingsBase<HotkeysConfig>
{
    public List<HotkeySettings> Hotkeys = HotkeyManager.GetDefaultHotkeyList();
}

