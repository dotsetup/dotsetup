// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    public class RichTextBoxEx : RichTextBox
    {
        #region Interop-Defines
        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public UInt16 wWeight;
            public UInt16 sSpacing;
            public int crBackColor; // Color.ToArgb() -> int
            public int lcid;
            public int dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_USER = 0x0400;
        private const int EM_GETCHARFORMAT = WM_USER + 58;
        private const int EM_SETCHARFORMAT = WM_USER + 68;

        private const int SCF_SELECTION = 0x0001;
        //private const int SCF_WORD = 0x0002;
        //private const int SCF_ALL = 0x0004;
        #endregion
        #region CHARFORMAT2 Flags
        //private const UInt32 CFE_BOLD = 0x0001;
        //private const UInt32 CFE_ITALIC = 0x0002;
        //private const UInt32 CFE_UNDERLINE = 0x0004;
        //private const UInt32 CFE_STRIKEOUT = 0x0008;
        //private const UInt32 CFE_PROTECTED = 0x0010;
        private const UInt32 CFE_LINK = 0x0020;
        //private const UInt32 CFE_AUTOCOLOR = 0x40000000;
        //private const UInt32 CFE_SUBSCRIPT = 0x00010000;        /* Superscript and subscript are */
        //private const UInt32 CFE_SUPERSCRIPT = 0x00020000;      /*  mutually exclusive			 */

        //private const int CFM_SMALLCAPS = 0x0040;           /* (*)	*/
        //private const int CFM_ALLCAPS = 0x0080;         /* Displayed by 3.0	*/
        //private const int CFM_HIDDEN = 0x0100;          /* Hidden by 3.0 */
        //private const int CFM_OUTLINE = 0x0200;         /* (*)	*/
        //private const int CFM_SHADOW = 0x0400;          /* (*)	*/
        //private const int CFM_EMBOSS = 0x0800;          /* (*)	*/
        //private const int CFM_IMPRINT = 0x1000;         /* (*)	*/
        //private const int CFM_DISABLED = 0x2000;
        //private const int CFM_REVISED = 0x4000;

        //private const int CFM_BACKCOLOR = 0x04000000;
        //private const int CFM_LCID = 0x02000000;
        //private const int CFM_UNDERLINETYPE = 0x00800000;       /* Many displayed by 3.0 */
        //private const int CFM_WEIGHT = 0x00400000;
        //private const int CFM_SPACING = 0x00200000;     /* Displayed by 3.0	*/
        //private const int CFM_KERNING = 0x00100000;     /* (*)	*/
        //private const int CFM_STYLE = 0x00080000;       /* (*)	*/
        //private const int CFM_ANIMATION = 0x00040000;       /* (*)	*/
        //private const int CFM_REVAUTHOR = 0x00008000;


        //private const UInt32 CFM_BOLD = 0x00000001;
        //private const UInt32 CFM_ITALIC = 0x00000002;
        //private const UInt32 CFM_UNDERLINE = 0x00000004;
        //private const UInt32 CFM_STRIKEOUT = 0x00000008;
        //private const UInt32 CFM_PROTECTED = 0x00000010;
        private const UInt32 CFM_LINK = 0x00000020;
        //private const UInt32 CFM_SIZE = 0x80000000;
        //private const UInt32 CFM_COLOR = 0x40000000;
        //private const UInt32 CFM_FACE = 0x20000000;
        //private const UInt32 CFM_OFFSET = 0x10000000;
        //private const UInt32 CFM_CHARSET = 0x08000000;
        //private const UInt32 CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
        //private const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

        //private const byte CFU_UNDERLINENONE = 0x00000000;
        //private const byte CFU_UNDERLINE = 0x00000001;
        //private const byte CFU_UNDERLINEWORD = 0x00000002; /* (*) displayed as ordinary underline	*/
        //private const byte CFU_UNDERLINEDOUBLE = 0x00000003; /* (*) displayed as ordinary underline	*/
        //private const byte CFU_UNDERLINEDOTTED = 0x00000004;
        //private const byte CFU_UNDERLINEDASH = 0x00000005;
        //private const byte CFU_UNDERLINEDASHDOT = 0x00000006;
        //private const byte CFU_UNDERLINEDASHDOTDOT = 0x00000007;
        //private const byte CFU_UNDERLINEWAVE = 0x00000008;
        //private const byte CFU_UNDERLINETHICK = 0x00000009;
        //private const byte CFU_UNDERLINEHAIRLINE = 0x0000000A; /* (*) displayed as ordinary underline	*/
        #endregion

        private const int WM_SETFOCUS = 0x0007;
        private const int WM_KILLFOCUS = 0x0008;
        private const int WM_SETCURSOR = 0x20;
        private const int WM_MOUSEWHEEL = 0x020A;
        private HorizontalAlignment _Alignment = HorizontalAlignment.Left;

        private struct HyperLinkText
        {
            public string text;
            public string hyperlink;
            public int startPosition;
        }

        readonly List<HyperLinkText> linkTextArray;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SETFOCUS:
                    m.Msg = WM_KILLFOCUS;
                    break;
                case WM_SETCURSOR: // stops the flickering on hover
                    m.Result = new IntPtr(1); //Signify that we dealt with the message. We should be returning "true", but I can't figure out how to do that.
                    return;
                case WM_MOUSEWHEEL:
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        m.WParam = IntPtr.Zero;
                        m.Result = IntPtr.Zero;
                    }
                    break;
                default:
                    break;
            }

            base.WndProc(ref m);
        }

        public RichTextBoxEx() : base()
        {
            // Otherwise, non-standard links get lost when user starts typing
            // next to a non-standard link
            DetectUrls = false;
            ReadOnly = true;
            linkTextArray = new List<HyperLinkText>();
            ScrollBars = RichTextBoxScrollBars.None;
            LinkClicked += new LinkClickedEventHandler(HandelLinkClicked);

            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            TextChanged += TransparentLabel_TextChanged;
        }

        void TransparentLabel_TextChanged(object sender, System.EventArgs e)
        {
            this.ForceRefresh();
        }

        public void ForceRefresh()
        {
            this.UpdateStyles();
        }

        [DefaultValue(false)]
        public new bool DetectUrls
        {
            get { return base.DetectUrls; }
            set { base.DetectUrls = value; }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                var OriginalSelectionAlignment = this.SelectionAlignment;
                base.Text = RemoveLinkBrackets(value);
                SetLinks();
                this.SelectAll();
                base.SelectionAlignment = OriginalSelectionAlignment;
                this.DeselectAll();
            }
        }

        public HorizontalAlignment Alignment
        {
            get
            {
                return _Alignment;
            }
            set
            {
                this._Alignment = value;
                this.SelectAll();
                this.SelectionAlignment = value;
                this.DeselectAll();
            }
        }


        /// <summary>
        /// Given that in the text there are hyperlink in the stucture 
        /// [Link_Text|Link_Url] it creates a pressable link from the text.
        /// </summary>
        private string RemoveLinkBrackets(string value)
        {
            value = value.Trim();
            while (value.Contains('['))
            {
                int startPos = value.IndexOf('[');
                int endPos = value.IndexOf(']');
                int seporatorPos = value.IndexOf('|');
                if (startPos < 0 || startPos > value.Length || startPos > endPos ||
                    endPos < 0 || endPos > value.Length || seporatorPos > endPos ||
                    seporatorPos < 0 || seporatorPos > value.Length)
                    break;
                HyperLinkText hyperLinkText;

                hyperLinkText.startPosition = startPos;
                hyperLinkText.text = value.Substring(startPos + 1, seporatorPos - startPos - 1);
                hyperLinkText.hyperlink = value.Substring(seporatorPos + 1, endPos - seporatorPos - 1).Trim();
                linkTextArray.Add(hyperLinkText);

                value = value.Remove(startPos, endPos - startPos + 1).Insert(startPos, hyperLinkText.text);
            }
            return value;
        }


        private void SetLinks()
        {
            foreach (HyperLinkText hyperLinkText in linkTextArray)
            {
                Select(hyperLinkText.startPosition, hyperLinkText.text.Length);
                SetSelectionLink(true);
            }
        }

        /// <summary>
        /// Set the current selection's link style
        /// </summary>
        /// <param name="link">true: set link style, false: clear link style</param>
        public void SetSelectionLink(bool link)
        {
            SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
        }
        /// <summary>
        /// Get the link style for the current selection
        /// </summary>
        /// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
        public int GetSelectionLink()
        {
            return GetSelectionStyle(CFM_LINK, CFE_LINK);
        }


        private void SetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.dwMask = mask;
            cf.dwEffects = effect;

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

            Marshal.FreeCoTaskMem(lpar);
        }

        private int GetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.szFaceName = new char[32];

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

            cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

            int state;
            // dwMask holds the information which properties are consistent throughout the selection:
            if ((cf.dwMask & mask) == mask)
            {
                if ((cf.dwEffects & effect) == effect)
                    state = 1;
                else
                    state = 0;
            }
            else
            {
                state = -1;
            }

            Marshal.FreeCoTaskMem(lpar);
            return state;
        }

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string s_File);
        public static IntPtr LoadLibrary(string s_File)
        {
            var module = LoadLibraryW(s_File);
            if (module != IntPtr.Zero)
                return module;
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; //This makes the control's background transparent
                try
                {
                    LoadLibrary("MsftEdit.dll"); // Available since XP SP1
                    cp.ClassName = "RichEdit50W";
                }
                catch { /* Windows XP without any Service Pack.*/ }
                return cp;
            }
        }

        internal void HandelLinkClicked(object sender, LinkClickedEventArgs e)
        {
            HyperLinkText linkText = linkTextArray.FirstOrDefault(x => x.text == e.LinkText);
            if (!String.IsNullOrEmpty(linkText.hyperlink))
            {
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    System.Diagnostics.Process.Start(linkText.hyperlink);
                });
                thread.Start();
            }
        }
    }
}
