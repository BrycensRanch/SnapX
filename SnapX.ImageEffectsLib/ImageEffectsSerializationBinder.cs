
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;

namespace ShareX.ImageEffectsLib
{
    public class ImageEffectsSerializationBinder : KnownTypesSerializationBinder
    {
        public ImageEffectsSerializationBinder()
        {
            KnownTypes = Helpers.FindSubclassesOf<ImageEffect>();
        }
    }
}