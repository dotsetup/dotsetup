// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Net;
using System.Reflection;
using System.Threading;

namespace DotSetup.Infrastructure
{
    public static class CommunicationUtils
    {
        public static string GetUA()
        {
            string os = Environment.OSVersion.Version.ToString();
            return $"Mozilla/5.0 (Windows NT {os.Substring(0, os.IndexOf('.', 3))}; {((OSUtils.Is64BitOperatingSystem()) ? "WOW64; " : "")}Trident/7.0; rv:11.0) like Gecko";
        }

        public static void EnableHighestTlsVersion()
        {
            Type type = Type.GetType("System.AppContext");
            if (type != null)
            {
                MethodInfo setSwitch = type.GetMethod("SetSwitch", BindingFlags.Public | BindingFlags.Static);
                setSwitch.Invoke(null, new object[] { "Switch.System.Net.DontEnableSystemDefaultTlsVersions", false });
            }
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xF00; // Allow variety of protocols to support different clients 
        }

        public static void HttpFireAndForget(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url) || !UriUtils.CheckURLValid(url))
                {
#if DEBUG
                    Logger.GetLogger().Error($"Invalid URL: {url}");
#endif
                    return;
                }
                url = url.Trim();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        request.GetResponse();
#if DEBUG
                        Logger.GetLogger().Info($"Sent http request to: {url}");
#endif
                    }
#if DEBUG
                    catch (Exception e)
#else
                    catch (Exception)
#endif
                    {
#if DEBUG
                        Logger.GetLogger().Error($"Error while sending Http reqeust to {url}: {e}");
#endif
                    }
                });
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Error while sending Http reqeust to {url}: {e}");
#endif
            }
        }

        public static void HttpPostAndForget(string url, string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url) || !UriUtils.CheckURLValid(url))
                {
#if DEBUG
                    Logger.GetLogger().Error($"Invalid URL: {url}");
#endif
                    return;
                }
                url = url.Trim();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {


                        using WebClient client = new WebClient();
                        client.UploadString(url, data);
#if DEBUG
                        Logger.GetLogger().Info($"Successfully sent post data to {url}, data: \n{data}");
#endif
                    }
#if DEBUG
                    catch (Exception e)
#else
                    catch (Exception)
#endif
                    {
#if DEBUG
                        Logger.GetLogger().Error($"Error while sending POST data to {url}: {e}");
#endif
                    }
                });
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Error while sending POST data to {url}: {e}");
#endif
            }
        }
    }
}
