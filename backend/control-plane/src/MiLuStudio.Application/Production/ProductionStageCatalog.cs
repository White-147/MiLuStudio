namespace MiLuStudio.Application.Production;

using MiLuStudio.Domain;

internal static class ProductionStageCatalog
{
    internal static readonly IReadOnlyList<ProductionStageDefinition> All =
    [
        new(ProductionStage.StoryIngesting, "story", "分析故事", "story_intake", "00:18", "¥0.03", NeedsReview: false),
        new(ProductionStage.ScriptReadyForReview, "script", "改编脚本", "episode_writer", "01:12", "¥0.21", NeedsReview: true),
        new(ProductionStage.CharacterReadyForReview, "character", "生成角色", "character_bible", "00:46", "¥0.18", NeedsReview: true),
        new(ProductionStage.StyleReadyForReview, "style", "生成风格", "style_bible", "--", "估算 ¥0.08", NeedsReview: true),
        new(ProductionStage.StoryboardReadyForReview, "storyboard", "生成分镜", "storyboard_director", "--", "估算 ¥0.16", NeedsReview: true),
        new(ProductionStage.ImagesReadyForReview, "image", "生成图片", "image_generation", "--", "估算 ¥1.80", NeedsReview: true),
        new(ProductionStage.VideosReadyForReview, "video", "生成视频", "video_generation", "--", "估算 ¥8.00", NeedsReview: true),
        new(ProductionStage.Exporting, "edit", "剪辑导出", "auto_editor", "--", "本地", NeedsReview: false)
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
