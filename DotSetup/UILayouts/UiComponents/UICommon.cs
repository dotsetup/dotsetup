// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.IO;

namespace DotSetup.UILayouts.UIComponents
{
    internal class UICommon
    {
        public static Image LoadImage(string imageName, string decode)
        {
            Image res = null;
            try
            {
                Stream imageStream;
                if (decode.ToLower() == CryptUtils.EncDec.BASE64)
                    imageStream = CryptUtils.Decode(imageName, decode);
                else
                    imageStream = ResourcesUtils.GetEmbeddedResourceStream(null, imageName);

                res = Image.FromStream(imageStream);
            }
#if DEBUG
            catch (Exception e)
#else
                catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"error while loading the image {imageName}: {e}");
#endif
            }

            return res;
        }
    }
}
