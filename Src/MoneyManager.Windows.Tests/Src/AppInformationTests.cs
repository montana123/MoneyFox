﻿using NUnit.Framework;

namespace MoneyManager.Windows.Tests
{
    [TestFixture]
    public class AppInformationTests
    {
        [Test]
        public void GetVersion_VersionInAppManifest_CorrectVersion()
        {
            new AppInformation().GetVersion.ShouldBe("1.0.0.0");
        }
    }
}