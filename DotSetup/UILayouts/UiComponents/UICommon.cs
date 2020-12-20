using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DotSetup.UILayouts.UIComponents
{
    class UICommon
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
