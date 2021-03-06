using System;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using TechTalk.SpecFlow;

namespace MyAccounts.Tests.Gmc.Steps
{
    [Binding]
    public class TestAccessorsTransforms
    {
        private readonly ScenarioContext _scenarioContext;

        public TestAccessorsTransforms(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [StepArgumentTransformation(@"ScenarioContext\:(.+)")]
        public Wrapper<string> ScenarioNameTransform(string currentContextProperty)
        {
            var accessor = GetAccessor<ScenarioContext, string>(currentContextProperty);
            return accessor(_scenarioContext);
        }

        [StepArgumentTransformation(@"Settings\:(.+)")]
        public Wrapper<string> AppSettingStringTransform(string key)
        {
            return AppSettingsTransform<string>(key);
        }

        [StepArgumentTransformation(@"Settings\:(.+)")]
        public PasswordWrapper AppSettingPasswordTransform(string key)
        {
            return AppSettingsTransform<string>(key);
        }

        private static T AppSettingsTransform<T>(string key)
        {
            var section = (ClientSettingsSection)ConfigurationManager.GetSection("applicationSettings/MyAccounts.Tests.Gmc.Properties.Settings");
            object value = section.Settings.Get(key).Value.ValueXml.InnerText;
            return (T)value;
        }

        private static Func<TOwner, TMember> GetAccessor<TOwner, TMember>(string path)
        {
            ParameterExpression paramObj = Expression.Parameter(typeof(TOwner), "obj");

            Expression body = path
                .Split('.')
                .Aggregate<string, Expression>(paramObj, Expression.PropertyOrField);

            return Expression.Lambda<Func<TOwner, TMember>>(body, paramObj).Compile();
        }
    }
}