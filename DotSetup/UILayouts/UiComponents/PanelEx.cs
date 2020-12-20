// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup
{
    public class PanelEx : Panel
    {
        public PanelEx() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        // Paint background with underlying graphics from other controls
        {
            base.OnPaintBackground(e);

            if (BackColor == System.Drawing.Color.Transparent)
            {
                Graphics g = e.Graphics;

                if (Parent != null)
                {
                    // Take each control in turn
                    int index = Parent.Controls.GetChildIndex(this);
                    for (int i = Parent.Controls.Count - 1; i > index; i--)
                    {
                        Control c = Parent.Controls[i];

                        // Check it's visible and overlaps this control
                        if (c.Bounds.IntersectsWith(Bounds) && c.Visible)
                        {
                            // Load appearance of underlying control and redraw it on this background
                            Bitmap bmp = new Bitmap(c.Width, c.Height, g);
                            c.DrawToBitmap(bmp, c.ClientRectangle);
                            g.TranslateTransform(c.Left - Left, c.Top - Top);
                            g.DrawImageUnscaled(bmp, Point.Empty);
                            g.TranslateTransform(Left - c.Left, Top - c.Top);
                            bmp.Dispose();
                        }
                    }
                }
            }
        }

        private Stream Base64ToStream(Stream responseStream, string decode)
        {
            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
            String responseString = reader.ReadToEnd();
            responseStream = CryptUtils.Decode(responseString, decode);
            return responseStream;
        }

        public void SetImage(string imageName, string decode)
        {
            string imageUrl = imageName;
            if (imageUrl.StartsWith("//"))
                imageUrl = "https:" + imageUrl;
            bool isImageFromUrl = Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (isImageFromUrl)
            {
                try
                {
                    WebRequest request = WebRequest.Create(imageUrl);
                    Action wrapperAction = () =>
                    {
                        request.BeginGetResponse(new AsyncCallback((arg) =>
                        {
                            var response = (HttpWebResponse)((HttpWebRequest)arg.AsyncState).EndGetResponse(arg);
                            Stream responseStream = response.GetResponseStream();
                            if (decode.ToLower() == CryptUtils.EncDec.BASE64)
                                responseStream = Base64ToStream(responseStream, decode);
                            BackgroundImage = System.Drawing.Image.FromStream(responseStream);
                        }), request);
                    };
                    wrapperAction.BeginInvoke(new AsyncCallback((arg) =>
                    {
                        var action = (Action)arg.AsyncState;
                        action.EndInvoke(arg);
                    }), wrapperAction);

                }
#if DEBUG
                catch (Exception e)
#else
                catch (Exception)
#endif
                {
#if DEBUG
                    Logger.GetLogger().Error("PanelEx SetImage error while trying to load from URL - " + e.Message);
#endif
                }
                finally
                {
                }
            }
            else
            {
                BackgroundImage = UICommon.LoadImage(imageName, decode);
            }
        }
    }
}
