namespace MiLuStudio.Application.Production;

internal static class ProductionStageCatalog
{
    internal static readonly IReadOnlyList<ProductionStageDefinition> All =
    [
        new("story", "分析故事", "story_intake", "00:18", "¥0.03", NeedsReview: false),
        new("script", "改编脚本", "episode_writer", "01:12", "¥0.21", NeedsReview: true),
        new("character", "生成角色", "character_bible", "00:46", "¥0.18", NeedsReview: true),
        new("style", "生成风格", "style_bible", "--", "估算 ¥0.08", NeedsReview: true),
        new("storyboard", "生成分镜", "storyboard_director", "--", "估算 ¥0.16", NeedsReview: true),
        new("image", "生成图片", "image_generation", "--", "估算 ¥1.80", NeedsReview: true),
        new("video", "生成视频", "video_generation", "--", "估算 ¥8.00", NeedsReview: true),
        new("edit", "剪辑导出", "auto_editor", "--", "本地", NeedsReview: false)
    ];
}

internal sealed record ProductionStageDefinition(
    string Id,
    string Label,
    string Skill,
    string Duration,
    string Cost,
    bool NeedsReview);
