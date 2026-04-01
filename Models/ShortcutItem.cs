namespace ImageEditor.Models
{
    public class ShortcutItem
    {
        public string Key { get; set; }
        public string Action { get; set; }
        public ShortcutItem(string key, string action)
        {
            Key = key;
            Action = action;
        }
    }
}