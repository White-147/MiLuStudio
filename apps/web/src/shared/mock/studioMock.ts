import type { DeliveryAsset, ProductionStage, ProjectSummary, ResultCard } from '../types/production';

export const mockProjects: ProjectSummary[] = [
  {
    id: 'demo-episode-01',
    title: '雨巷里的纸鹤',
    description: '悬疑都市短篇，女孩追踪会发光的纸鹤，发现失踪哥哥留下的线索。',
    mode: 'director',
    status: 'running',
    targetDuration: 45,
    aspectRatio: '9:16',
    updatedAt: '2026-05-12 16:42',
    progress: 62,
  },
  {
    id: 'demo-episode-02',
    title: '星港便利店',
    description: '轻喜剧科幻，一家夜班便利店每天凌晨都会接待不同星球的客人。',
    mode: 'fast',
    status: 'draft',
    targetDuration: 35,
    aspectRatio: '9:16',
    updatedAt: '2026-05-12 14:10',
    progress: 18,
  },
  {
    id: 'demo-episode-03',
    title: '长安旧梦',
    description: '古风奇幻，画师进入一幅未完成的壁画，替画中少年改写结局。',
    mode: 'director',
    status: 'completed',
    targetDuration: 58,
    aspectRatio: '9:16',
    updatedAt: '2026-05-11 21:30',
    progress: 100,
  },
];

export const mockStages: ProductionStage[] = [
  { id: 'story', label: '分析故事', skill: 'story_intake', status: 'done', duration: '00:18', cost: '¥0.03', needsReview: false },
  { id: 'script', label: '改编脚本', skill: 'episode_writer', status: 'review', duration: '01:12', cost: '¥0.21', needsReview: true },
  { id: 'character', label: '生成角色', skill: 'character_bible', status: 'running', duration: '00:46', cost: '¥0.18', needsReview: true },
  { id: 'style', label: '生成风格', skill: 'style_bible', status: 'waiting', duration: '--', cost: '估算 ¥0.08', needsReview: true },
  { id: 'storyboard', label: '生成分镜', skill: 'storyboard_director', status: 'waiting', duration: '--', cost: '估算 ¥0.16', needsReview: true },
  { id: 'image', label: '生成图片', skill: 'image_generation', status: 'waiting', duration: '--', cost: '估算 ¥1.80', needsReview: true },
  { id: 'video', label: '生成视频', skill: 'video_generation', status: 'waiting', duration: '--', cost: '估算 ¥8.00', needsReview: true },
  { id: 'edit', label: '剪辑导出', skill: 'auto_editor', status: 'waiting', duration: '--', cost: '本地', needsReview: false },
];

export const mockResultCards: ResultCard[] = [
  {
    id: 'script-card',
    title: '脚本卡',
    kind: 'script',
    status: 'ready',
    summary: '45 秒竖屏结构，三段式推进，保留悬念钩子。',
    details: ['旁白 6 句', '对白 4 句', '结尾保留反转镜头'],
  },
  {
    id: 'character-card',
    title: '角色卡',
    kind: 'character',
    status: 'draft',
    summary: '主角、哥哥、纸鹤引路人三组设定已生成。',
    details: ['主角：短发、雨衣、旧相机', '哥哥：只在线索和回忆中出现', '纸鹤：暖白发光材质'],
  },
  {
    id: 'storyboard-card',
    title: '分镜卡',
    kind: 'storyboard',
    status: 'draft',
    summary: '默认 8 个镜头，总时长 45 秒。',
    details: ['开场雨巷俯拍', '中段跟拍纸鹤', '末尾推近旧相机照片'],
  },
  {
    id: 'media-card',
    title: '图片 / 视频卡',
    kind: 'media',
    status: 'draft',
    summary: '等待角色和分镜确认后生成首帧、尾帧和视频片段。',
    details: ['图生视频优先', '单镜头可重试', '保留成本记录'],
  },
];

export const mockDeliveryAssets: DeliveryAsset[] = [
  { id: 'mp4', label: '最终成片', format: 'MP4', size: '--', state: 'waiting' },
  { id: 'srt', label: '字幕文件', format: 'SRT', size: '--', state: 'waiting' },
  { id: 'storyboard', label: '分镜表', format: 'XLSX', size: '42 KB', state: 'ready' },
  { id: 'assets', label: '素材包', format: 'ZIP', size: '--', state: 'waiting' },
];
