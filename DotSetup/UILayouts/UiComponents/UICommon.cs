// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.IO;
using System.Net;
using DotSetup.Infrastructure;

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

                if (imageStream != null)
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

        public static Stream Base64ToStream(Stream responseStream, string decode)
        {
            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
            string responseString = reader.ReadToEnd();
            responseStream = CryptUtils.Decode(responseString, decode);
            return responseStream;
        }

        public static Image PrepareImageResponse(Stream responseStream, string decode)
        {
            if (decode.ToLower() == CryptUtils.EncDec.BASE64)
                responseStream = Base64ToStream(responseStream, decode);
            return Image.FromStream(responseStream);
        }

        public static Image PrepareImage(string imageData, string decode, Action<Image> asyncCallback = null)
        {
            Image image = null;

            if (string.IsNullOrWhiteSpace(imageData))
                return image;

            if (imageData.StartsWith("//"))
                imageData = "https:" + imageData;
            bool isImageFromUrl = Uri.TryCreate(imageData, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (isImageFromUrl)
            {
                try
                {
                    WebRequest request = WebRequest.Create(imageData);

#if DEBUG
                    Logger.GetLogger().Info($"loading image from URL: {imageData}");
#endif
                    if (asyncCallback == null) //blocking
                    {
                        image = PrepareImageResponse(request.GetResponse().GetResponseStream(), decode);
                    }
                    else
                    {
                        Action wrapperAction = () =>
                        {                            
                                request.BeginGetResponse(new AsyncCallback((arg) =>
                                {
                                    try
                                    {
                                        var response = (HttpWebResponse)((HttpWebRequest)arg.AsyncState).EndGetResponse(arg);
                                        image = PrepareImageResponse(response.GetResponseStream(), decode);
                                        asyncCallback?.Invoke(image);
                                    }
#if DEBUG
                                    catch (Exception e)
#else
                                    catch (Exception)
#endif
                                    {
#if DEBUG
                                        Logger.GetLogger().Error($"loading image from URL ({imageData}) error while downloading - {e.Message}");
#endif
                                    }
                                }), request);                            
                        };
                        
                        wrapperAction.BeginInvoke(new AsyncCallback((arg) =>
                        {
                            var action = (Action)arg.AsyncState;
                            action.EndInvoke(arg);
                        }), wrapperAction);
                    }
                }
#if DEBUG
                catch (Exception e)
#else
                catch (Exception)
#endif
                {
#if DEBUG
                    Logger.GetLogger().Error("loading image from URL error - " + e.Message);
#endif
                }                
            }
            else
            {
                image = LoadImage(imageData, decode);
            }
            return image;
        }

    }
}
