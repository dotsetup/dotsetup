// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup
{
    public class PictureBoxEx : PictureBox
    {
        public PictureBoxEx() : base()
        {

        }


        public void SetImage(string imageName, string decode)
        {
            string imageUrl = imageName;
            if (imageUrl.StartsWith("//"))
                imageUrl = "https:" + imageUrl;
            bool isImageFromUrl = Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isImageFromUrl)
            {
                // running asynchronously the function pictureBox.Load(imageName) 
                Action<string> asyncAction = Load;
                asyncAction.BeginInvoke(imageUrl, ar =>
                {
                    try
                    {
                        asyncAction.EndInvoke(ar);
                    }
#if DEBUG
                    catch (Exception e)
#else
                    catch (Exception)
#endif
                    {
#if DEBUG
                        Logger.GetLogger().Error("Error in PictureBox " + Name + " while Loading from URL: " + e.Message);
#endif
                    }
                    finally
                    {
                    }
                }, null);
            }
            else
            {
                Image = UICommon.LoadImage(imageName, decode);
            }
        }
    }
}
