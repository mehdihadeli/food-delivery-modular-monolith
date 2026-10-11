namespace Tests.Shared.XunitCategories;

/// <summary>
/// Enables filtering via 'dotnet test --filter "Category=Unit"' etc.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CategoryTraitAttribute : Attribute
{
    public CategoryTraitAttribute(TestCategory name) => Name = name;

    // Exposed as trait "Category"
    public TestCategory Name { get; }

    // For traditional trait-key for test filters: "Category"
    public string Category => Name.ToString();
}

/// <summary>
/// Stable category values shared by unit, integration and end-to-end test projects.
/// </summary>
public enum TestCategory
{
    Unit,
    Integration,
    EndToEnd,
    LoadTest,
    SkipCI,
}

/// <summary>
/// Marks a test as covering a known defect/regression.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class BugTraitAttribute : Attribute
{
    public BugTraitAttribute(string bugId) => BugId = bugId;

    public string BugId { get; }

    public string Category => "Bug";
}

/// <summary>
/// Marks a test with the feature it belongs to, for feature-based filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class FeatureTraitAttribute : Attribute
{
    public FeatureTraitAttribute(string featureName) => FeatureName = featureName;

    public string FeatureName { get; }

    public string Category => "Feature";
}
