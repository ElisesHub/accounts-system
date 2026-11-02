using System.Diagnostics;

namespace AccountsAPI.Presentation.Helpers;

public static class TraceContextHelper
{
    public static string CurrentTraceId()
    {
        return Activity.Current?.TraceId.ToString() ?? "Unavailable";

    }


}