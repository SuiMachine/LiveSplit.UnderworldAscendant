using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UnderworldAscendant
{
    public partial class UnderworldAscendantSettings : UserControl
    {
        public bool UseNonSafeMemoryReading { get; set; }


        private const bool DEFAULT_UNSAFEREADER = true;

        public UnderworldAscendantSettings()
        {
            InitializeComponent();

            // defaults
            this.UseNonSafeMemoryReading = DEFAULT_UNSAFEREADER;
        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            this.UseNonSafeMemoryReading = ParseBool(settings, "NonSafeMemoryReader", DEFAULT_UNSAFEREADER);
        }

        static bool ParseBool(XmlNode settings, string setting, bool default_ = false)
        {
            bool val;
            return settings[setting] != null ?
                (Boolean.TryParse(settings[setting].InnerText, out val) ? val : default_)
                : default_;
        }

        static XmlElement ToElement<T>(XmlDocument document, string name, T value)
        {
            XmlElement str = document.CreateElement(name);
            str.InnerText = value.ToString();
            return str;
        }
    }
}
