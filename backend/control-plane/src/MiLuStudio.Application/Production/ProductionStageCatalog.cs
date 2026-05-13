namespace MiLuStudio.Application.Production;

using MiLuStudio.Domain;

internal static class ProductionStageCatalog
{
    internal static readonly IReadOnlyList<ProductionStageDefinition> All =
    [
        new(ProductionStage.StoryIngesting, "story", "Story intake", "story_intake", "00:18", "local", NeedsReview: false),
        new(ProductionStage.PlotAdapted, "plot", "Plot adaptation", "plot_adaptation", "00:24", "local", NeedsReview: false),
        new(ProductionStage.ScriptReadyForReview, "script", "Script review", "episode_writer", "01:12", "local", NeedsReview: true),
        new(ProductionStage.CharacterReadyForReview, "character", "Character bible", "character_bible", "00:46", "local", NeedsReview: true),
        new(ProductionStage.StyleReadyForReview, "style", "Style bible", "style_bible", "--", "local", NeedsReview: true),
        new(ProductionStage.StoryboardReadyForReview, "storyboard", "Storyboard", "storyboard_director", "--", "local", NeedsReview: true),
        new(ProductionStage.ImagePromptsReady, "image_prompt", "Image prompts", "image_prompt_builder", "--", "local", NeedsReview: false),
        new(ProductionStage.ImagesReadyForReview, "image", "Mock images", "image_generation", "--", "local", NeedsReview: true),
        new(ProductionStage.VideoPromptsReady, "video_prompt", "Video prompts", "video_prompt_builder", "--", "local", NeedsReview: false),
        new(ProductionStage.VideosReadyForReview, "video", "Mock videos", "video_generation", "--", "local", NeedsReview: true),
        new(ProductionStage.AudioReadyForReview, "voice", "Voice casting", "voice_casting", "--", "local", NeedsReview: true),
        new(ProductionStage.SubtitlesReady, "subtitle", "Subtitle structure", "subtitle_generator", "--", "local", NeedsReview: false),
        new(ProductionStage.EditReadyForQualityCheck, "edit", "Rough edit plan", "auto_editor", "--", "local", NeedsReview: false),
        new(ProductionStage.QualityReadyForReview, "quality", "Quality report", "quality_checker", "--", "local", NeedsReview: true),
        new(ProductionStage.Exporting, "export", "Export package", "export_packager", "--", "local", NeedsReview: false)
    ];

    internal static ProductionStageDefinition First => All[0];

    internal static ProductionStageDefinition? Find(ProductionStage stage)
    {
        return All.FirstOrDefault(definition => definition.Stage == stage);
    }

    internal static ProductionStageDefinition? FindBySkill(string skill)
    {
        return All.FirstOrDefault(definition => string.Equals(definition.Skill, skill, StringComparison.OrdinalIgnoreCase));
    }

    internal static int IndexOf(ProductionStage stage)
    {
        for (var index = 0; index < All.Count; index++)
        {
            if (All[index].Stage == stage)
            {
                return index;
            }
        }

        return -1;
    }

    internal static int ProgressFor(ProductionStage stage)
    {
        if (stage == ProductionStage.Completed)
        {
            return 100;
        }

        var index = IndexOf(stage);
        return index < 0 ? 0 : (int)Math.Round((index + 1) * 100m / All.Count);
    }

    internal static string ExternalIdFor(ProductionStage stage)
    {
        var definition = Find(stage);

        if (definition is not null)
        {
            return definition.Id;
        }

        return stage switch
        {
            ProductionStage.Created => "created",
            ProductionStage.Completed => "completed",
            ProductionStage.FailedRetryable => "failed_retryable",
            ProductionStage.FailedNeedsUser => "failed_needs_user",
            ProductionStage.FailedFatal => "failed_fatal",
            _ => "unknown"
        };
    }
}

internal sealed record ProductionStageDefinition(
    ProductionStage Stage,
    string Id,
    string Label,
    string Skill,
    string Duration,
    string Cost,
    bool NeedsReview);
