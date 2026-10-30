using Microsoft.AspNetCore.Components.Web;

namespace AttendanceUI.Components;

/// <summary>
/// Keyboard support for elements that act as buttons without being a &lt;button&gt; (cards,
/// table rows, timetable cells, thumbnails). Pair with <c>role="button" tabindex="0"</c> and
/// an <c>@onkeydown</c> handler that checks <see cref="IsActivation"/>; index.html stops
/// Space from also scrolling the page.
/// </summary>
public static class KeyActivation
{
    /// <summary>True for the keys that press a button: Enter and Space.</summary>
    public static bool IsActivation(KeyboardEventArgs e) => e.Key is "Enter" or " " or "Spacebar";
}
