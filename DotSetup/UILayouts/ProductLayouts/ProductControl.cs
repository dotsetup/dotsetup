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
