using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace WhoOwesWho.Shared.Attributes
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();

                fixture.Customize(new AutoMoqCustomization
                {
                    ConfigureMembers = true
                });

                return fixture;
            })
        {
        }
    }
}
