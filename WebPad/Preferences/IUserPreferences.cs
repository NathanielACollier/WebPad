namespace WebPad.Preferences
{
    public interface IUserPreferences
    {
        void LoadPreferences(Properties.Settings settings);
        void SavePreferences(Properties.Settings settings);
    }
}