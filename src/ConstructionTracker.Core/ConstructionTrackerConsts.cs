using ConstructionTracker.Debugging;

namespace ConstructionTracker;

public class ConstructionTrackerConsts
{
    public const string LocalizationSourceName = "ConstructionTracker";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "2053c9db590e4886a11204e5a7ad01f2";
}
