using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaEdit.Document
{
    public struct DocumentLineFormat
    {
        #region Tag
        /// <summary>
        /// Gets/Sets the tag for the line format
        /// </summary>
        public string Tag { get; set; }
        #endregion

        #region Margins
        /// <summary>
        /// Gets/Sets the line's margins (in px)
        /// </summary>
		public Thickness Margins { get; set; }
        #endregion

        #region Line Spacing
        private double _lineSpacingPercentage;

        /// <summary>
        /// Gets/Sets the line spacing percentage for this line
        /// </summary>
        public double LineSpacingPercentage
        {
            get => _lineSpacingPercentage;
            set
            {
                _lineSpacingPercentage = value;
                HasLineSpacingOverride = true;
            }
        }

        /// <summary>
        /// Gets/Sets if this line has a line spacing override
        /// </summary>
        public bool HasLineSpacingOverride { get; private set; }

        /// <summary>
        /// Clears the line spacing override for this line
        /// </summary>
        public void ClearLineSpacing()
        {
            LineSpacingPercentage = 0.0;
            HasLineSpacingOverride = false;
        }
        #endregion

        /*#region Font Size
        private double _fontSize;

        /// <summary>
        /// Gets/Sets the font size override for this line
        /// </summary>
        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                HasFontSizeOverride = true;
            }
        }

        /// <summary>
        /// Gets if this line has a font size override
        /// </summary>
        public bool HasFontSizeOverride { get; private set; }

        /// <summary>
        /// Clears the font size override for this line
        /// </summary>
        public void ClearFontSize()
        {
            FontSize = 0.0;
            HasFontSizeOverride = false;
        }
        #endregion

        #region Typeface
        private string _typeface;

        /// <summary>
        /// Gets/Sets the typeface for this line
        /// </summary>
        public string Typeface
        {
            get => _typeface;
            set => _typeface = value;
        }

        /// <summary>
        /// Gets if this line has a typeface override
        /// </summary>
        public bool HasTypefaceOverride => !string.IsNullOrEmpty(Typeface);

        /// <summary>
        /// Clears the typeface override for this line
        /// </summary>
        public void ClearTypeface()
        {
            Typeface = string.Empty;
        }
        #endregion*/

        #region Case Transforming
        public enum TextCasingMode
        {
            Default = 0,
            AllLower,
            AllUpper
        }

        /// <summary>
        /// Gets/Sets the text casing override for this line
        /// </summary>
        public TextCasingMode TextCasing { get; set; }
        #endregion

        public string ToCss()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Tag)) sb.Append($"tag: {Tag}; ");
            sb.Append($"margins: {Margins}; ");
            if (HasLineSpacingOverride) sb.Append($"line-spacing: {LineSpacingPercentage}; ");
            //if (HasFontSizeOverride) sb.Append($"font-size: {FontSize}; ");
            //if (HasTypefaceOverride) sb.Append($"font-family: {Typeface}; ");

            switch (TextCasing)
            {
                case TextCasingMode.AllLower:
                    sb.Append($"text-transform: lowercase; ");
                    break;
                case TextCasingMode.AllUpper:
                    sb.Append($"text-transform: uppercase; ");
                    break;
                default:
                    sb.Append($"text-transform: none; ");
                    break;
            }

            return sb.ToString();
        }

        public static DocumentLineFormat ParseCss(string css)
        {
            DocumentLineFormat format = new DocumentLineFormat();
            string[] parts = css.Split(';');

            for (int i = 0; i < parts.Length; i++)
            {
                string[] styleParts = parts[i].Split(':');

                if (styleParts.Length != 2) continue;

                switch (styleParts[0].Trim())
                {
                    case "tag":
                        format.Tag = styleParts[1].Trim();
                        break;
                    case "margins":
                        format.Margins = Thickness.Parse(styleParts[1].Trim());
                        break;
                    case "line-spacing":
                        format.LineSpacingPercentage = double.Parse(styleParts[1]);
                        break;
                    //case "font-size":
                    //    format.FontSize = double.Parse(styleParts[1]);
                    //    break;
                    //case "font-family":
                    //    format.Typeface = styleParts[1].Trim();
                    //    break;
                    case "text-transform":
                        switch (styleParts[1].Trim())
                        {
                            case "uppercase":
                                format.TextCasing = TextCasingMode.AllUpper;
                                break;
                            case "lowercase":
                                format.TextCasing = TextCasingMode.AllLower;
                                break;
                            default: break;
                        }
                        break;
                    default: break;
                }
            }

            return format;
        }
    }
}
