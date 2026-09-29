namespace EthicalLab.Shared
{
    public enum HubPage { Office, Home, Missions, Terminal, Case, Glossary }

    /// <summary>
    /// Navegación del menú ESC. La UI no decide reglas de misión; solo qué pantalla mostrar.
    /// </summary>
    public sealed class HubPageRouter
    {
        public HubPage Page { get; private set; } = HubPage.Office;
        public bool MenuDirty { get; private set; } = true;
        public bool MenuOpen => Page != HubPage.Office;

        public void ToggleEscape()
        {
            Page = Page == HubPage.Office ? HubPage.Home : HubPage.Office;
            if (Page != HubPage.Office) MenuDirty = true;
        }

        public void Open(string target)
        {
            Page = Parse(target);
            MenuDirty = true;
        }

        public void MarkDirty() => MenuDirty = true;

        public void ConsumeRebuild() => MenuDirty = false;

        public static HubPage Parse(string target)
        {
            switch (target)
            {
                case "office": return HubPage.Office;
                case "terminal": return HubPage.Terminal;
                case "glossary": return HubPage.Glossary;
                case "missions": return HubPage.Missions;
                case "case": return HubPage.Case;
                case "home": return HubPage.Home;
                default: return HubPage.Home;
            }
        }
    }
}
