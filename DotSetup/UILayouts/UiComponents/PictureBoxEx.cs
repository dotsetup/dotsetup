// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

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
            Image = UICommon.PrepareImage(imageName, decode);
        }
    }
}
