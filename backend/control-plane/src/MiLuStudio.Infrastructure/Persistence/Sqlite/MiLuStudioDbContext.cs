namespace MiLuStudio.Infrastructure.Persistence.Sqlite;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class MiLuStudioDbContext : DbContext
{
    public MiLuStudioDbContext(DbContextOptions<MiLuStudioDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToStringConverter>();
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<StoryInput> StoryInputs => Set<StoryInput>();

    public DbSet<ProductionJob> ProductionJobs => Set<ProductionJob>();

    public DbSet<GenerationTask> GenerationTasks => Set<GenerationTask>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<CostLedgerEntry> CostLedger => Set<CostLedgerEntry>();

    public DbSet<Character> Characters => Set<Character>();

    public DbSet<Shot> Shots => Set<Shot>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    public DbSet<DeviceBinding> DeviceBindings => Set<DeviceBinding>();

    public DbSet<LicenseGrant> LicenseGrants => Set<LicenseGrant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureProjects(modelBuilder);
        ConfigureStoryInputs(modelBuilder);
        ConfigureProductionJobs(modelBuilder);
        ConfigureGenerationTasks(modelBuilder);
        ConfigureAssets(modelBuilder);
        ConfigureCostLedger(modelBuilder);
        ConfigureCharacters(modelBuilder);
        ConfigureShots(modelBuilder);
        ConfigureAccounts(modelBuilder);
        ConfigureAuthSessions(modelBuilder);
        ConfigureDeviceBindings(modelBuilder);
        ConfigureLicenseGrants(modelBuilder);
    }

    private static void ConfigureProjects(ModelBuilder modelBuilder)
    {
        var modeConverter = new ValueConverter<ProjectMode, string>(
            value => ToProjectModeValue(value),
            value => FromProjectModeValue(value));
        var statusConverter = new ValueConverter<ProjectStatus, string>(
            value => ToProjectStatusValue(value),
            value => FromProjectStatusValue(value));

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(project => project.Id);
            entity.Property(project => project.Id).HasColumnName("id");
            entity.Property(project => project.Name).HasColumnName("name");
            entity.Property(project => project.Description).HasColumnName("description");
            entity.Property(project => project.Mode).HasColumnName("mode").HasConversion(modeConverter);
            entity.Property(project => project.Status).HasColumnName("status").HasConversion(statusConverter);
            entity.Property(project => project.TargetDurationSeconds).HasColumnName("target_duration_seconds");
            entity.Property(project => project.AspectRatio).HasColumnName("aspect_ratio");
            entity.Property(project => project.StylePreset).HasColumnName("style_preset");
            entity.Property(project => project.CreatedAt).HasColumnName("created_at");
            entity.Property(project => project.UpdatedAt).HasColumnName("updated_at");
        });
    }

    private static void ConfigureStoryInputs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoryInput>(entity =>
        {
            entity.ToTable("story_inputs");
            entity.HasKey(story => story.Id);
            entity.Property(story => story.Id).HasColumnName("id");
            entity.Property(story => story.ProjectId).HasColumnName("project_id");
            entity.Property(story => story.SourceType).HasColumnName("source_type");
            entity.Property(story => story.OriginalText).HasColumnName("original_text");
            entity.Property(story => story.FileAssetId).HasColumnName("file_asset_id");
            entity.Property(story => story.Language).HasColumnName("language");
            entity.Property(story => story.WordCount).HasColumnName("word_count");
            entity.Property(story => story.ParsedAt).HasColumnName("parsed_at");
        });
    }

    private static void ConfigureProductionJobs(ModelBuilder modelBuilder)
    {
        var stageConverter = new ValueConverter<ProductionStage, string>(
            value => ToStageValue(value),
            value => FromStageValue(value));
        var statusConverter = new ValueConverter<ProductionJobStatus, string>(
            value => ToProductionJobStatusValue(value),
            value => FromProductionJobStatusValue(value));

        modelBuilder.Entity<ProductionJob>(entity =>
        {
            entity.ToTable("production_jobs");
            entity.HasKey(job => job.Id);
            entity.Property(job => job.Id).HasColumnName("id");
            entity.Property(job => job.ProjectId).HasColumnName("project_id");
            entity.Property(job => job.CurrentStage).HasColumnName("current_stage").HasConversion(stageConverter);
            entity.Property(job => job.Status).HasColumnName("status").HasConversion(statusConverter);
            entity.Property(job => job.ProgressPercent).HasColumnName("progress_percent");
            entity.Property(job => job.StartedAt).HasColumnName("started_at");
            entity.Property(job => job.FinishedAt).HasColumnName("finished_at");
            entity.Property(job => job.ErrorMessage).HasColumnName("error_message");
        });
    }

    private static void ConfigureGenerationTasks(ModelBuilder modelBuilder)
    {
        var statusConverter = new ValueConverter<GenerationTaskStatus, string>(
            value => ToGenerationTaskStatusValue(value),
            value => FromGenerationTaskStatusValue(value));

        modelBuilder.Entity<GenerationTask>(entity =>
        {
            entity.ToTable("generation_tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Id).HasColumnName("id");
            entity.Property(task => task.JobId).HasColumnName("job_id");
            entity.Property(task => task.ProjectId).HasColumnName("project_id");
            entity.Property(task => task.ShotId).HasColumnName("shot_id");
            entity.Property(task => task.QueueIndex).HasColumnName("queue_index");
            entity.Property(task => task.SkillName).HasColumnName("skill_name");
            entity.Property(task => task.Provider).HasColumnName("provider");
            entity.Property(task => task.InputJson).HasColumnName("input_json").HasColumnType("TEXT");
            entity.Property(task => task.OutputJson).HasColumnName("output_json").HasColumnType("TEXT");
            entity.Property(task => task.Status).HasColumnName("status").HasConversion(statusConverter);
            entity.Property(task => task.AttemptCount).HasColumnName("attempt_count");
            entity.Property(task => task.CostEstimate).HasColumnName("cost_estimate");
            entity.Property(task => task.CostActual).HasColumnName("cost_actual");
            entity.Property(task => task.StartedAt).HasColumnName("started_at");
            entity.Property(task => task.FinishedAt).HasColumnName("finished_at");
            entity.Property(task => task.LockedBy).HasColumnName("locked_by");
            entity.Property(task => task.LockedUntil).HasColumnName("locked_until");
            entity.Property(task => task.LastHeartbeatAt).HasColumnName("last_heartbeat_at");
            entity.Property(task => task.CheckpointNotes).HasColumnName("checkpoint_notes");
            entity.Property(task => task.ErrorMessage).HasColumnName("error_message");
        });
    }

    private static void ConfigureAssets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets");
            entity.HasKey(asset => asset.Id);
            entity.Property(asset => asset.Id).HasColumnName("id");
            entity.Property(asset => asset.ProjectId).HasColumnName("project_id");
            entity.Property(asset => asset.Kind).HasColumnName("kind");
            entity.Property(asset => asset.LocalPath).HasColumnName("local_path");
            entity.Property(asset => asset.MimeType).HasColumnName("mime_type");
            entity.Property(asset => asset.FileSize).HasColumnName("file_size");
            entity.Property(asset => asset.Sha256).HasColumnName("sha256");
            entity.Property(asset => asset.MetadataJson).HasColumnName("metadata_json").HasColumnType("TEXT");
            entity.Property(asset => asset.CreatedAt).HasColumnName("created_at");
        });
    }

    private static void ConfigureCostLedger(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CostLedgerEntry>(entity =>
        {
            entity.ToTable("cost_ledger");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasColumnName("id");
            entity.Property(entry => entry.ProjectId).HasColumnName("project_id");
            entity.Property(entry => entry.TaskId).HasColumnName("task_id");
            entity.Property(entry => entry.Provider).HasColumnName("provider");
            entity.Property(entry => entry.Model).HasColumnName("model");
            entity.Property(entry => entry.Unit).HasColumnName("unit");
            entity.Property(entry => entry.Quantity).HasColumnName("quantity");
            entity.Property(entry => entry.EstimatedCost).HasColumnName("estimated_cost");
            entity.Property(entry => entry.ActualCost).HasColumnName("actual_cost");
            entity.Property(entry => entry.CreatedAt).HasColumnName("created_at");
        });
    }

    private static void ConfigureCharacters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("characters");
            entity.HasKey(character => character.Id);
            entity.Property(character => character.Id).HasColumnName("id");
            entity.Property(character => character.ProjectId).HasColumnName("project_id");
            entity.Property(character => character.Name).HasColumnName("name");
            entity.Property(character => character.RoleType).HasColumnName("role_type");
            entity.Property(character => character.Gender).HasColumnName("gender");
            entity.Property(character => character.AgeRange).HasColumnName("age_range");
            entity.Property(character => character.Personality).HasColumnName("personality");
            entity.Property(character => character.Appearance).HasColumnName("appearance");
            entity.Property(character => character.Costume).HasColumnName("costume");
            entity.Property(character => character.VoiceProfile).HasColumnName("voice_profile");
            entity.Property(character => character.ConsistencyNotes).HasColumnName("consistency_notes");
        });
    }

    private static void ConfigureShots(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shot>(entity =>
        {
            entity.ToTable("shots");
            entity.HasKey(shot => shot.Id);
            entity.Property(shot => shot.Id).HasColumnName("id");
            entity.Property(shot => shot.ProjectId).HasColumnName("project_id");
            entity.Property(shot => shot.EpisodeId).HasColumnName("episode_id");
            entity.Property(shot => shot.ShotIndex).HasColumnName("shot_index");
            entity.Property(shot => shot.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(shot => shot.SceneSummary).HasColumnName("scene_summary");
            entity.Property(shot => shot.Dialogue).HasColumnName("dialogue");
            entity.Property(shot => shot.Narration).HasColumnName("narration");
            entity.Property(shot => shot.CharactersJson).HasColumnName("characters_json").HasColumnType("TEXT");
            entity.Property(shot => shot.CameraAngle).HasColumnName("camera_angle");
            entity.Property(shot => shot.CameraMotion).HasColumnName("camera_motion");
            entity.Property(shot => shot.Lighting).HasColumnName("lighting");
            entity.Property(shot => shot.Composition).HasColumnName("composition");
            entity.Property(shot => shot.ImagePrompt).HasColumnName("image_prompt");
            entity.Property(shot => shot.VideoPrompt).HasColumnName("video_prompt");
            entity.Property(shot => shot.Status).HasColumnName("status");
            entity.Property(shot => shot.UserLocked).HasColumnName("user_locked");
        });
    }

    private static void ConfigureAccounts(ModelBuilder modelBuilder)
    {
        var statusConverter = new ValueConverter<AccountStatus, string>(
            value => ToAccountStatusValue(value),
            value => FromAccountStatusValue(value));

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Id).HasColumnName("id");
            entity.Property(account => account.Email).HasColumnName("email");
            entity.Property(account => account.Phone).HasColumnName("phone");
            entity.Property(account => account.DisplayName).HasColumnName("display_name");
            entity.Property(account => account.PasswordHash).HasColumnName("password_hash");
            entity.Property(account => account.Status).HasColumnName("status").HasConversion(statusConverter);
            entity.Property(account => account.CreatedAt).HasColumnName("created_at");
            entity.Property(account => account.LastLoginAt).HasColumnName("last_login_at");
        });
    }

    private static void ConfigureAuthSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("auth_sessions");
            entity.HasKey(session => session.Id);
            entity.Property(session => session.Id).HasColumnName("id");
            entity.Property(session => session.AccountId).HasColumnName("account_id");
            entity.Property(session => session.DeviceId).HasColumnName("device_id");
            entity.Property(session => session.AccessTokenHash).HasColumnName("access_token_hash");
            entity.Property(session => session.RefreshTokenHash).HasColumnName("refresh_token_hash");
            entity.Property(session => session.CreatedAt).HasColumnName("created_at");
            entity.Property(session => session.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(session => session.ExpiresAt).HasColumnName("expires_at");
            entity.Property(session => session.RevokedAt).HasColumnName("revoked_at");
        });
    }

    private static void ConfigureDeviceBindings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceBinding>(entity =>
        {
            entity.ToTable("devices");
            entity.HasKey(device => device.Id);
            entity.Property(device => device.Id).HasColumnName("id");
            entity.Property(device => device.AccountId).HasColumnName("account_id");
            entity.Property(device => device.MachineFingerprintHash).HasColumnName("machine_fingerprint_hash");
            entity.Property(device => device.DeviceName).HasColumnName("device_name");
            entity.Property(device => device.FirstSeenAt).HasColumnName("first_seen_at");
            entity.Property(device => device.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(device => device.Trusted).HasColumnName("trusted");
        });
    }

    private static void ConfigureLicenseGrants(ModelBuilder modelBuilder)
    {
        var typeConverter = new ValueConverter<LicenseKind, string>(
            value => ToLicenseKindValue(value),
            value => FromLicenseKindValue(value));
        var statusConverter = new ValueConverter<LicenseStatus, string>(
            value => ToLicenseStatusValue(value),
            value => FromLicenseStatusValue(value));

        modelBuilder.Entity<LicenseGrant>(entity =>
        {
            entity.ToTable("licenses");
            entity.HasKey(license => license.Id);
            entity.Property(license => license.Id).HasColumnName("id");
            entity.Property(license => license.AccountId).HasColumnName("account_id");
            entity.Property(license => license.LicenseType).HasColumnName("license_type").HasConversion(typeConverter);
            entity.Property(license => license.Plan).HasColumnName("plan");
            entity.Property(license => license.ActivationCodeHash).HasColumnName("activation_code_hash");
            entity.Property(license => license.Status).HasColumnName("status").HasConversion(statusConverter);
            entity.Property(license => license.StartsAt).HasColumnName("starts_at");
            entity.Property(license => license.ExpiresAt).HasColumnName("expires_at");
            entity.Property(license => license.MaxDevices).HasColumnName("max_devices");
            entity.Property(license => license.CreatedAt).HasColumnName("created_at");
            entity.Property(license => license.UpdatedAt).HasColumnName("updated_at");
        });
    }

    internal static string ToStageValue(ProductionStage value)
    {
        return value switch
        {
            ProductionStage.StoryIngesting => "story_ingesting",
            ProductionStage.PlotAdapted => "plot_adapted",
            ProductionStage.ScriptReadyForReview => "script_ready_for_review",
            ProductionStage.CharacterReadyForReview => "character_ready_for_review",
            ProductionStage.StyleReadyForReview => "style_ready_for_review",
            ProductionStage.StoryboardReadyForReview => "storyboard_ready_for_review",
            ProductionStage.ImagePromptsReady => "image_prompts_ready",
            ProductionStage.ImagesReadyForReview => "images_ready_for_review",
            ProductionStage.VideoPromptsReady => "video_prompts_ready",
            ProductionStage.VideosReadyForReview => "videos_ready_for_review",
            ProductionStage.AudioReadyForReview => "audio_ready_for_review",
            ProductionStage.SubtitlesReady => "subtitles_ready",
            ProductionStage.EditReadyForQualityCheck => "edit_ready_for_quality_check",
            ProductionStage.QualityReadyForReview => "quality_ready_for_review",
            ProductionStage.Exporting => "exporting",
            ProductionStage.Completed => "completed",
            ProductionStage.FailedRetryable => "failed_retryable",
            ProductionStage.FailedNeedsUser => "failed_needs_user",
            ProductionStage.FailedFatal => "failed_fatal",
            _ => "created"
        };
    }

    internal static ProductionStage FromStageValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "story_ingesting" => ProductionStage.StoryIngesting,
            "plot_adapted" => ProductionStage.PlotAdapted,
            "script_ready_for_review" => ProductionStage.ScriptReadyForReview,
            "character_ready_for_review" => ProductionStage.CharacterReadyForReview,
            "style_ready_for_review" => ProductionStage.StyleReadyForReview,
            "storyboard_ready_for_review" => ProductionStage.StoryboardReadyForReview,
            "image_prompts_ready" => ProductionStage.ImagePromptsReady,
            "images_ready_for_review" => ProductionStage.ImagesReadyForReview,
            "video_prompts_ready" => ProductionStage.VideoPromptsReady,
            "videos_ready_for_review" => ProductionStage.VideosReadyForReview,
            "audio_ready_for_review" => ProductionStage.AudioReadyForReview,
            "subtitles_ready" => ProductionStage.SubtitlesReady,
            "edit_ready_for_quality_check" => ProductionStage.EditReadyForQualityCheck,
            "quality_ready_for_review" => ProductionStage.QualityReadyForReview,
            "exporting" => ProductionStage.Exporting,
            "completed" => ProductionStage.Completed,
            "failed_retryable" => ProductionStage.FailedRetryable,
            "failed_needs_user" => ProductionStage.FailedNeedsUser,
            "failed_fatal" => ProductionStage.FailedFatal,
            _ => ProductionStage.Created
        };
    }

    private static string ToProjectModeValue(ProjectMode value)
    {
        return value == ProjectMode.Fast ? "fast" : "director";
    }

    private static ProjectMode FromProjectModeValue(string value)
    {
        return string.Equals(value, "fast", StringComparison.OrdinalIgnoreCase) ? ProjectMode.Fast : ProjectMode.Director;
    }

    private static string ToProjectStatusValue(ProjectStatus value)
    {
        return value switch
        {
            ProjectStatus.Running => "running",
            ProjectStatus.Paused => "paused",
            ProjectStatus.Completed => "completed",
            ProjectStatus.Failed => "failed",
            _ => "draft"
        };
    }

    private static ProjectStatus FromProjectStatusValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "running" => ProjectStatus.Running,
            "paused" => ProjectStatus.Paused,
            "completed" => ProjectStatus.Completed,
            "failed" => ProjectStatus.Failed,
            _ => ProjectStatus.Draft
        };
    }

    private static string ToProductionJobStatusValue(ProductionJobStatus value)
    {
        return value switch
        {
            ProductionJobStatus.Running => "running",
            ProductionJobStatus.Paused => "paused",
            ProductionJobStatus.Completed => "completed",
            ProductionJobStatus.Failed => "failed",
            _ => "queued"
        };
    }

    private static ProductionJobStatus FromProductionJobStatusValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "running" => ProductionJobStatus.Running,
            "paused" => ProductionJobStatus.Paused,
            "completed" => ProductionJobStatus.Completed,
            "failed" => ProductionJobStatus.Failed,
            _ => ProductionJobStatus.Queued
        };
    }

    private static string ToGenerationTaskStatusValue(GenerationTaskStatus value)
    {
        return value switch
        {
            GenerationTaskStatus.Running => "running",
            GenerationTaskStatus.Review => "review",
            GenerationTaskStatus.Completed => "completed",
            GenerationTaskStatus.Failed => "failed",
            _ => "waiting"
        };
    }

    private static GenerationTaskStatus FromGenerationTaskStatusValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "running" => GenerationTaskStatus.Running,
            "review" => GenerationTaskStatus.Review,
            "completed" => GenerationTaskStatus.Completed,
            "failed" => GenerationTaskStatus.Failed,
            _ => GenerationTaskStatus.Waiting
        };
    }

    private static string ToAccountStatusValue(AccountStatus value)
    {
        return value switch
        {
            AccountStatus.Locked => "locked",
            AccountStatus.Deleted => "deleted",
            _ => "active"
        };
    }

    private static AccountStatus FromAccountStatusValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "locked" => AccountStatus.Locked,
            "deleted" => AccountStatus.Deleted,
            _ => AccountStatus.Active
        };
    }

    private static string ToLicenseKindValue(LicenseKind value)
    {
        return value switch
        {
            LicenseKind.Trial => "trial",
            LicenseKind.OfflineSigned => "offline_signed",
            _ => "paid"
        };
    }

    private static LicenseKind FromLicenseKindValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "trial" => LicenseKind.Trial,
            "offline_signed" => LicenseKind.OfflineSigned,
            _ => LicenseKind.Paid
        };
    }

    private static string ToLicenseStatusValue(LicenseStatus value)
    {
        return value switch
        {
            LicenseStatus.Expired => "expired",
            LicenseStatus.Revoked => "revoked",
            _ => "active"
        };
    }

    private static LicenseStatus FromLicenseStatusValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "expired" => LicenseStatus.Expired,
            "revoked" => LicenseStatus.Revoked,
            _ => LicenseStatus.Active
        };
    }
}
