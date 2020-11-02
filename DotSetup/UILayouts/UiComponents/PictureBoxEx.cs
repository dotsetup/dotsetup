// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

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
                    {
                        Logger.GetLogger().Error("Error in PictureBox " + Name + " while Loading from URL: " + e.Message);
                    }
#endif
                    finally
                    {
                    }
                }, null);
            }
            else
            {
                try
                {
                    System.IO.Stream imageStream;
                    if (decode.ToLower() == CryptUtils.EncDec.BASE64)
                        imageStream = CryptUtils.Decode(imageName, decode);
                    else
                        imageStream = ResourcesUtils.GetEmbeddedResourceStream(ResourcesUtils.wrapperAssembly, imageName);
                    Image = System.Drawing.Image.FromStream(imageStream);
                }
#if DEBUG
                catch (Exception e)
                {
                    Logger.GetLogger().Error("Error in PictureBox " + Name + " while Loading image from resource: " + e.Message);
                }
#endif
                finally
                {
                }
            }
        }
    }
}