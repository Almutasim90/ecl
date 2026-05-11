namespace ECL.Models;

/// <summary>
/// Page model for the Grammar Guide. Centralizes section and tense anchor metadata so
/// the table of contents, deep links, and future routes (e.g. topic pages) stay in sync.
/// </summary>
public sealed class GrammarGuidePageViewModel
{
    public IReadOnlyList<GrammarGuideSectionNav> Sections { get; init; } = Array.Empty<GrammarGuideSectionNav>();
    public IReadOnlyList<GrammarGuideTenseNav> Tenses { get; init; } = Array.Empty<GrammarGuideTenseNav>();
}

public sealed record GrammarGuideSectionNav(
    string AnchorId,
    string Title,
    string? Summary = null,
    string? IconClass = null);

/// <param name="TimeKey">Matches tab panels: present, past, future.</param>
public sealed record GrammarGuideTenseNav(
    string AnchorId,
    string Title,
    string TimeKey);

/// <summary>
/// Single source of truth for Grammar Guide navigation. Add entries here when new
/// major sections or tenses are introduced; keep matching <c>id</c> attributes in the view.
/// </summary>
public static class GrammarGuideCatalog
{
    public const string PageTopAnchor = "grammar-guide";

    public static IReadOnlyList<GrammarGuideSectionNav> MainSections { get; } =
    [
        new(PageTopAnchor, "Overview", "Intro and how to use this page.", "bi-book"),
        new("grammar-quick-test", "Quick test", "Warm up with three questions.", "bi-lightning-charge"),
        new("grammar-tenses-overview", "Tenses at a glance", "Explore all 12 tenses by time frame.", "bi-grid-3x3-gap"),
        new("grammar-timeline", "Tense timeline", "Where each tense sits on the timeline.", "bi-sliders2"),
        new("grammar-summary-table", "Full comparison", "One table for structure and usage.", "bi-table"),
    ];

    public static IReadOnlyList<GrammarGuideTenseNav> Tenses { get; } =
    [
        new("tense-present-simple", "Present Simple", "present"),
        new("tense-present-continuous", "Present Continuous", "present"),
        new("tense-present-perfect", "Present Perfect", "present"),
        new("tense-present-perfect-continuous", "Present Perfect Continuous", "present"),
        new("tense-past-simple", "Past Simple", "past"),
        new("tense-past-continuous", "Past Continuous", "past"),
        new("tense-past-perfect", "Past Perfect", "past"),
        new("tense-past-perfect-continuous", "Past Perfect Continuous", "past"),
        new("tense-future-simple", "Future Simple", "future"),
        new("tense-future-continuous", "Future Continuous", "future"),
        new("tense-future-perfect", "Future Perfect", "future"),
        new("tense-future-perfect-continuous", "Future Perfect Continuous", "future"),
    ];

    public static GrammarGuidePageViewModel CreatePageModel() => new()
    {
        Sections = MainSections,
        Tenses = Tenses,
    };
}
