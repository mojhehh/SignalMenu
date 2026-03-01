using System;
using System.Text;

namespace SignalMenu.Classes
{
    internal static class ObfStr
    {
        private static readonly byte _k = 0xA7;

        private static string D(byte[] d)
        {
            byte[] r = new byte[d.Length];
            for (int i = 0; i < d.Length; i++)
                r[i] = (byte)(d[i] ^ _k ^ (byte)(i * 3));
            return Encoding.UTF8.GetString(r);
        }

        private static string _menuName;
        public static string MenuName
        {
            get
            {
                if (_menuName == null)
                    _menuName = D(new byte[] { 0xF4, 0xCD, 0xC6, 0xC0, 0xCA, 0xC4, 0x95, 0xFF, 0xDA, 0xD2, 0xCC });
                return _menuName;
            }
        }

        private static string _menuNameBold;
        public static string MenuNameBold
        {
            get
            {
                if (_menuNameBold == null)
                    _menuNameBold = "<b>" + MenuName + "</b>";
                return _menuNameBold;
            }
        }

        private static string _menuNameOrange;
        public static string MenuNameOrange
        {
            get
            {
                if (_menuNameOrange == null)
                    _menuNameOrange = "<b><color=#FF8000>" + MenuName + "</color></b>";
                return _menuNameOrange;
            }
        }

        private static string _menuNameBlue;
        public static string MenuNameBlue
        {
            get
            {
                if (_menuNameBlue == null)
                    _menuNameBlue = "<color=blue><b>" + MenuName + " </b></color>";
                return _menuNameBlue;
            }
        }

        private static string _menuNameGrey;
        public static string MenuNameGrey
        {
            get
            {
                if (_menuNameGrey == null)
                    _menuNameGrey = "<color=grey><b>" + MenuName + " </b></color>";
                return _menuNameGrey;
            }
        }

        private static string _fileTag;
        public static string FileTag
        {
            get
            {
                if (_fileTag == null)
                    _fileTag = D(new byte[] { 0xF3, 0xC1, 0xD9, 0xDA, 0x8B, 0xCE, 0xDC, 0xDE, 0xDA, 0x9C, 0xDE, 0xE3, 0xED, 0xE5, 0xFF, 0xEB, 0xE3, 0xF1, 0xF5, 0xBE, 0xEC, 0xF1, 0x91, 0x8A, 0xCF });
                return _fileTag + MenuName;
            }
        }

        private static string _devName;
        public static string DevName
        {
            get
            {
                if (_devName == null)
                    _devName = D(new byte[] { 0xCC, 0xCD, 0xCF, 0xC9, 0xC4, 0xCE, 0xDB, 0xD7, 0xCB, 0xDA, 0xD5, 0xEF, 0xFB });
                return _devName;
            }
        }

        private static string _forkCredit;
        public static string ForkCredit
        {
            get
            {
                if (_forkCredit == null)
                    _forkCredit = "ii's Stupid Menu";
                return _forkCredit;
            }
        }
    }
}
