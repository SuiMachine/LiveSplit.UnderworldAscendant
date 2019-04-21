using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UnderworldAscendant
{
    public partial class UnderworldAscendantSettings : UserControl
    {
        public bool SplitOnLevelChange { get; set; }
        public int RescansLimit { get; set; }

        //Defaults
        private const bool DEFAULT_SPLIT_ONLEVELCHANGE = false;
        private const int DEFAULT_RESCANS_LIMIT = 0;

        public UnderworldAscendantSettings()
        {
            InitializeComponent();

            this.CB_SplitOnLevelChange.DataBindings.Add("Checked", this, "SplitOnLevelChange", false, DataSourceUpdateMode.OnPropertyChanged);
            this.NumUpDn_RescansLimit.DataBindings.Add("Value", this, "RescansLimit", false, DataSourceUpdateMode.OnPropertyChanged);

            // defaults
            this.SplitOnLevelChange = DEFAULT_SPLIT_ONLEVELCHANGE;
            this.RescansLimit = DEFAULT_RESCANS_LIMIT;
        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));

            settingsNode.AppendChild(ToElement(doc, "SplitOnLevelChange", this.SplitOnLevelChange));
            settingsNode.AppendChild(ToElement(doc, "RescansLimit", this.RescansLimit));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            this.SplitOnLevelChange = ParseBool(settings, "SplitOnLevelChange", DEFAULT_SPLIT_ONLEVELCHANGE);
            this.RescansLimit = ParseInt(settings, "RescansLimit", DEFAULT_RESCANS_LIMIT);
        }

        static bool ParseBool(XmlNode settings, string setting, bool default_ = false)
        {
            bool val;
            return settings[setting] != null ?
                (Boolean.TryParse(settings[setting].InnerText, out val) ? val : default_)
                : default_;
        }

        static int ParseInt(XmlNode settings, string setting, int default_ = 0)
        {
            int val;
            return settings[setting] != null ?
                (int.TryParse(settings[setting].InnerText, out val) ? val : default_)
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
