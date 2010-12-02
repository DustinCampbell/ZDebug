using System;
using System.Windows;
using ZDebug.UI.Controls;
namespace ZDebug.UI.Services
{
    internal static class FontsAndColorsService
    {
        private static readonly Uri uri = new Uri("/ZDebug.UI;component/Views/Styles.xaml", UriKind.Relative);
        private static readonly ResourceDictionary resources = Application.LoadComponent(uri) as ResourceDictionary;

        private static readonly FontAndColorSetting addressSetting = (FontAndColorSetting)resources["AddressSetting"];
        private static readonly FontAndColorSetting commentSetting = (FontAndColorSetting)resources["CommentSetting"];
        private static readonly FontAndColorSetting constantSetting = (FontAndColorSetting)resources["ConstantSetting"];
        private static readonly FontAndColorSetting globalVariableSetting = (FontAndColorSetting)resources["GlobalVariableSetting"];
        private static readonly FontAndColorSetting keywordSetting = (FontAndColorSetting)resources["KeywordSetting"];
        private static readonly FontAndColorSetting localVariableSetting = (FontAndColorSetting)resources["LocalVariableSetting"];
        private static readonly FontAndColorSetting separatorSetting = (FontAndColorSetting)resources["SeparatorSetting"];
        private static readonly FontAndColorSetting stackVariableSetting = (FontAndColorSetting)resources["StackVariableSetting"];
        private static readonly FontAndColorSetting ztextSetting = (FontAndColorSetting)resources["ZTextSetting"];

        public static FontAndColorSetting AddressSetting
        {
            get { return addressSetting; }
        }

        public static FontAndColorSetting CommentSetting
        {
            get { return commentSetting; }
        }

        public static FontAndColorSetting ConstantSetting
        {
            get { return constantSetting; }
        }

        public static FontAndColorSetting GlobalVariableSetting
        {
            get { return globalVariableSetting; }
        }

        public static FontAndColorSetting KeywordSetting
        {
            get { return keywordSetting; }
        }

        public static FontAndColorSetting LocalVariableSetting
        {
            get { return localVariableSetting; }
        }

        public static FontAndColorSetting SeparatorSetting
        {
            get { return separatorSetting; }
        }

        public static FontAndColorSetting StackVariableSetting
        {
            get { return stackVariableSetting; }
        }

        public static FontAndColorSetting ZTextSetting
        {
            get { return ztextSetting; }
        }
    }
}
