using System.ComponentModel;

namespace QuickSetup.UI.Infra
{
    public enum SoftwareInstallStatusEnum
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("Not Installed")]
        NotInstalled = 1,

        [Description("Installed")]
        Installed = 2,

        [Description("Setup File Missing")]
        SetupFileMissing = 3,

        [Description("Unable To Get Status")]
        UnableToGetStatus = 4,

        [Description("Different Version Is Installed")]
        DifferentVersionDetected = 5
    }
}