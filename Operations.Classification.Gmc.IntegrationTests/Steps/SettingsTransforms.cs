using Operations.Classification.Gmc.IntegrationTests.Properties;
using TechTalk.SpecFlow;

namespace Operations.Classification.Gmc.IntegrationTests.Steps
{
    [Binding]
    public class SettingsTransforms
    {
        [StepArgumentTransformation(@"Settings\:(.+)")]
        public Wrapper<string> AppSettingStringTransform(string key)
        {
            return AppSettingsTransform<string>(key);
        }

        private static T AppSettingsTransform<T>(string key)
        {
            return (T)Settings.Default[key];
        }
    }
}