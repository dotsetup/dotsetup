// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{
    public class ProductControl : UserControl
    {
        public virtual void HandleChanges() { }
        public virtual void HandleChanges(object sender, EventArgs e) { }
    }
}
