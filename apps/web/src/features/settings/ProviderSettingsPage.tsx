import { KeyRound, Power, RefreshCcw, Save, ShieldCheck, SlidersHorizontal, Trash2 } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import {
  getProviderSettings,
  getProviderSettingsPreflight,
  testProviderConnection,
  updateProviderSettings,
} from '../../shared/api/controlPlaneClient';
import type {
  ProviderAdapterSettings,
  ProviderCostGuardrails,
  ProviderPreflightCheck,
  ProviderSafetyStatus,
  ProviderSettingsPreflight,
  ProviderSettingsResponse,
} from '../../shared/types/production';

type LoadState = 'idle' | 'loading' | 'saving';

const supplierLabels: Record<string, string> = {
  none: '未选择',
  openai: 'OpenAI',
  qwen: '通义千问',
  volcano: '火山引擎',
  anthropic: 'Anthropic',
  stability: 'Stability',
  runway: 'Runway',
  pika: 'Pika',
  kling: '可灵',
  elevenlabs: 'ElevenLabs',
  minimax: 'MiniMax',
  azure: 'Azure',
  openai_compatible: 'OpenAI 兼容中转',
};

const capabilityLabels: Record<string, string> = {
  character: '角色',
  keyframe: '关键帧',
  rewrite: '改写',
  script: '脚本',
  storyboard: '分镜',
  story: '故事',
  'style-reference': '风格参考',
};

const safetyModeLabels: Record<string, string> = {
  connection_test_allowed_generation_blocked: '仅允许连接测试，生成保持关闭',
  local_user_encrypted_metadata: '本地用户加密存储',
  project_local_dpapi: '项目本地加密存储',
  stage22_no_provider_calls: '生成调用关闭',
  stage23_connection_test_only: '仅连接测试',
  stage23_windows_dpapi: 'Windows 本地加密',
};

const providerPreflightLabels: Record<string, string> = {
  secret_store: '本地密钥存储',
  spend_guard: '成本保护',
  provider_sandbox: '调用边界',
};

const providerStatusLabels: Record<string, string> = {
  error: '异常',
  ok: '正常',
  skipped: '未启用',
  warning: '待补齐',
};

type ConnectionTestState = {
  busy: boolean;
  ok: boolean | null;
  message: string;
};

export function ProviderSettingsPage() {
  const [settings, setSettings] = useState<ProviderSettingsResponse | null>(null);
  const [draftAdapters, setDraftAdapters] = useState<ProviderAdapterSettings[]>([]);
  const [draftCost, setDraftCost] = useState<ProviderCostGuardrails>({
    projectCostCapCny: 50,
    retryLimit: 1,
  });
  const [apiKeyInputs, setApiKeyInputs] = useState<Record<string, string>>({});
  const [clearApiKeys, setClearApiKeys] = useState<Record<string, boolean>>({});
  const [connectionTests, setConnectionTests] = useState<Record<string, ConnectionTestState>>({});
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [message, setMessage] = useState('正在读取模型配置');

  const preflight = settings?.preflight ?? emptyPreflight;
  const safety = settings?.safety ?? emptySafety;
  const enabledCount = useMemo(() => draftAdapters.filter((adapter) => adapter.enabled).length, [draftAdapters]);

  useEffect(() => {
    const controller = new AbortController();
    void loadSettings(controller.signal);
    return () => controller.abort();
  }, []);

  const loadSettings = async (signal?: AbortSignal) => {
    setLoadState('loading');
    try {
      const nextSettings = await getProviderSettings(signal);
      applySettings(nextSettings);
      setMessage('模型配置已同步');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '读取模型配置失败');
    } finally {
      setLoadState('idle');
    }
  };

  const refreshPreflight = async () => {
    setLoadState('loading');
    try {
      const nextPreflight = await getProviderSettingsPreflight();
      setSettings((current) => (current ? { ...current, preflight: nextPreflight } : current));
      setMessage(nextPreflight.healthy ? '模型预检通过' : '模型预检需要处理');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '模型预检失败');
    } finally {
      setLoadState('idle');
    }
  };

  const saveSettings = async () => {
    setLoadState('saving');
    try {
      const nextSettings = await updateProviderSettings({
        costGuardrails: draftCost,
        adapters: draftAdapters.map((adapter) => ({
          kind: adapter.kind,
          supplier: adapter.supplier,
          model: adapter.model,
          baseUrl: adapter.baseUrl,
          enabled: adapter.enabled,
          apiKey: apiKeyInputs[adapter.kind]?.trim() || null,
          clearApiKey: Boolean(clearApiKeys[adapter.kind]),
        })),
      });
      applySettings(nextSettings);
      setMessage('模型配置已保存，生成调用仍保持关闭');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '保存模型配置失败');
    } finally {
      setLoadState('idle');
    }
  };

  const applySettings = (nextSettings: ProviderSettingsResponse) => {
    setSettings(nextSettings);
    setDraftAdapters(nextSettings.adapters);
    setDraftCost(nextSettings.costGuardrails);
    setApiKeyInputs({});
    setClearApiKeys({});
    setConnectionTests({});
  };

  const updateAdapter = <K extends keyof ProviderAdapterSettings>(
    kind: string,
    field: K,
    value: ProviderAdapterSettings[K],
  ) => {
    setDraftAdapters((current) =>
      current.map((adapter) => (adapter.kind === kind ? { ...adapter, [field]: value } : adapter)),
    );
  };

  const updateApiKeyInput = (kind: string, value: string) => {
    setApiKeyInputs((current) => ({ ...current, [kind]: value }));
    setClearApiKeys((current) => ({ ...current, [kind]: false }));
  };

  const clearApiKey = (kind: string) => {
    setApiKeyInputs((current) => ({ ...current, [kind]: '' }));
    setClearApiKeys((current) => ({ ...current, [kind]: true }));
    setDraftAdapters((current) =>
      current.map((adapter) =>
        adapter.kind === kind
          ? { ...adapter, apiKeyConfigured: false, apiKeyPreview: '', secretFingerprint: '' }
          : adapter,
      ),
    );
  };

  const testConnection = async (adapter: ProviderAdapterSettings) => {
    setConnectionTests((current) => ({
      ...current,
      [adapter.kind]: { busy: true, ok: null, message: '正在测试连接...' },
    }));

    try {
      const result = await testProviderConnection(adapter.kind, {
        supplier: adapter.supplier,
        model: adapter.model,
        baseUrl: adapter.baseUrl,
        apiKey: apiKeyInputs[adapter.kind]?.trim() || null,
      });
      setConnectionTests((current) => ({
        ...current,
        [adapter.kind]: {
          busy: false,
          ok: result.ok,
          message: result.ok ? `连接成功，耗时 ${result.durationMs}ms` : result.message,
        },
      }));
    } catch (error) {
      setConnectionTests((current) => ({
        ...current,
        [adapter.kind]: {
          busy: false,
          ok: false,
          message: error instanceof Error ? error.message : '连接测试失败',
        },
      }));
    }
  };

  return (
    <section className="provider-settings-view">
      <div className="provider-settings-heading">
        <div>
          <p className="eyebrow">本地模型服务</p>
          <h1>模型配置</h1>
        </div>
        <div className="provider-settings-actions">
          <span className={preflight.healthy ? 'api-chip connected' : 'api-chip'}>
            {preflight.healthy ? '预检通过' : '预检待处理'}
          </span>
          <button
            className="secondary-button"
            disabled={loadState !== 'idle'}
            onClick={() => void refreshPreflight()}
            type="button"
          >
            <RefreshCcw size={17} />
            <span>预检</span>
          </button>
          <button
            className="primary-button"
            disabled={loadState !== 'idle'}
            onClick={() => void saveSettings()}
            type="button"
          >
            <Save size={17} />
            <span>{loadState === 'saving' ? '保存中' : '保存'}</span>
          </button>
        </div>
      </div>

      <p className={preflight.healthy ? 'diagnostics-message ok' : 'diagnostics-message'}>{message}</p>

      <div className="provider-summary-strip">
        <div>
          <strong>{enabledCount}</strong>
          <span>已启用接口</span>
        </div>
        <div>
          <strong>{draftCost.projectCostCapCny.toFixed(2)}</strong>
          <span>单项目成本上限 CNY</span>
        </div>
        <div>
          <strong>{draftCost.retryLimit}</strong>
          <span>失败重试次数</span>
        </div>
        <div>
          <strong>{safety.sandbox.providerCallsAllowed ? '开放' : '阻断'}</strong>
          <span>生成调用</span>
        </div>
      </div>

      <div className="provider-settings-layout">
        <div className="provider-adapter-list">
          {draftAdapters.map((adapter) => (
            <article className={adapter.enabled ? 'provider-adapter-card enabled' : 'provider-adapter-card'} key={adapter.kind}>
              <div className="provider-adapter-heading">
                <div className="provider-adapter-title">
                  <span className="result-icon">
                    <KeyRound size={17} />
                  </span>
                  <div>
                    <h2>{adapter.label}</h2>
                    <p>{formatCapabilityFlags(adapter.capabilityFlags)}</p>
                  </div>
                </div>
                <label className="settings-toggle">
                  <input
                    checked={adapter.enabled}
                    onChange={(event) => updateAdapter(adapter.kind, 'enabled', event.target.checked)}
                    type="checkbox"
                  />
                  <span>
                    <Power size={14} />
                  </span>
                </label>
              </div>

              <div className="provider-adapter-fields">
                <label>
                  <span>供应商</span>
                  <select
                    onChange={(event) => updateAdapter(adapter.kind, 'supplier', event.target.value)}
                    value={adapter.supplier}
                  >
                    {adapter.supportedSuppliers.map((supplier) => (
                      <option key={supplier} value={supplier}>
                        {supplierLabels[supplier] ?? supplier}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  <span>默认模型</span>
                  <input
                    onChange={(event) => updateAdapter(adapter.kind, 'model', event.target.value)}
                    value={adapter.model}
                  />
                </label>
                <label className="wide-field">
                  <span>接口地址</span>
                  <input
                    onChange={(event) => updateAdapter(adapter.kind, 'baseUrl', event.target.value)}
                    placeholder="https://relay.example.com/v1"
                    value={adapter.baseUrl}
                  />
                </label>
                <label className="wide-field">
                  <span>API Key</span>
                  <input
                    autoComplete="off"
                    onChange={(event) => updateApiKeyInput(adapter.kind, event.target.value)}
                    placeholder={adapter.apiKeyConfigured ? adapter.apiKeyPreview : '未配置'}
                    type="password"
                    value={apiKeyInputs[adapter.kind] ?? ''}
                  />
                </label>
              </div>

              <div className="provider-secret-row">
                <span>
                  {apiKeyInputs[adapter.kind]
                    ? '待保存新密钥'
                    : adapter.apiKeyConfigured
                      ? `已配置 ${adapter.apiKeyPreview}`
                      : '未配置密钥'}
                </span>
                <small>{formatSafetyMode(adapter.safety.secretStoreMode)}</small>
                <button
                  className="ghost-button"
                  disabled={connectionTests[adapter.kind]?.busy || (!adapter.apiKeyConfigured && !apiKeyInputs[adapter.kind])}
                  onClick={() => void testConnection(adapter)}
                  type="button"
                >
                  <RefreshCcw size={15} />
                  <span>{connectionTests[adapter.kind]?.busy ? '测试中' : '测试连通'}</span>
                </button>
                <button
                  className="ghost-button"
                  disabled={!adapter.apiKeyConfigured && !apiKeyInputs[adapter.kind]}
                  onClick={() => clearApiKey(adapter.kind)}
                  type="button"
                >
                  <Trash2 size={15} />
                  <span>清除</span>
                </button>
              </div>
              {connectionTests[adapter.kind]?.message && (
                <p className={connectionTests[adapter.kind]?.ok ? 'provider-test-message ok' : 'provider-test-message'}>
                  {connectionTests[adapter.kind]?.message}
                </p>
              )}
            </article>
          ))}
        </div>

        <aside className="provider-side-panel">
          <section className="provider-settings-panel">
            <div className="provider-panel-title">
              <SlidersHorizontal size={17} />
              <h2>成本边界</h2>
            </div>
            <label>
              <span>单项目成本上限 CNY</span>
              <input
                min={0}
                onChange={(event) =>
                  setDraftCost((current) => ({
                    ...current,
                    projectCostCapCny: Number(event.target.value),
                  }))
                }
                step="0.01"
                type="number"
                value={draftCost.projectCostCapCny}
              />
            </label>
            <label>
              <span>失败重试次数</span>
              <input
                max={5}
                min={0}
                onChange={(event) =>
                  setDraftCost((current) => ({
                    ...current,
                    retryLimit: Number(event.target.value),
                  }))
                }
                step={1}
                type="number"
                value={draftCost.retryLimit}
              />
            </label>
          </section>

          <section className="provider-settings-panel">
            <div className="provider-panel-title">
              <ShieldCheck size={17} />
              <h2>安全前置层</h2>
            </div>
            <div className="provider-safety-list">
              <SafetyRow
                label="密钥存储"
                status={safety.secretStore.rawSecretPersistenceAllowed ? 'error' : 'ok'}
                value={formatSafetyMode(safety.secretStore.mode)}
              />
              <SafetyRow
                label="成本保护"
                status={safety.spendGuard.enabled ? 'ok' : 'error'}
                value={formatSafetyMode(safety.spendGuard.enforcementMode)}
              />
              <SafetyRow
                label="调用边界"
                status={safety.sandbox.providerCallsAllowed ? 'error' : 'ok'}
                value={formatSafetyMode(safety.sandbox.mode)}
              />
            </div>
          </section>

          <section className="provider-settings-panel">
            <div className="provider-panel-title">
              <ShieldCheck size={17} />
              <h2>本地预检</h2>
            </div>
            <div className="provider-preflight-list">
              {preflight.checks.map((check) => (
                <article className="provider-preflight-row" key={check.kind}>
                  <div>
                    <strong>{formatProviderCheckLabel(check)}</strong>
                    <p>{formatProviderCheckMessage(check)}</p>
                  </div>
                  <span className={`status-pill ${check.status}`}>{formatProviderStatus(check.status)}</span>
                </article>
              ))}
            </div>
          </section>
        </aside>
      </div>
    </section>
  );
}

function SafetyRow({ label, status, value }: { label: string; status: 'ok' | 'error'; value: string }) {
  return (
    <article className="provider-safety-row">
      <div>
        <strong>{label}</strong>
        <p>{value}</p>
      </div>
      <span className={`status-pill ${status}`}>{formatProviderStatus(status)}</span>
    </article>
  );
}

function formatCapabilityFlags(flags: string[]): string {
  return flags.map((flag) => capabilityLabels[flag] ?? flag).join(' / ');
}

function formatSafetyMode(value: string): string {
  return safetyModeLabels[value] ?? value.replaceAll('_', ' ');
}

function formatProviderCheckLabel(check: ProviderPreflightCheck): string {
  return providerPreflightLabels[check.kind] ?? check.label;
}

function formatProviderCheckMessage(check: ProviderPreflightCheck): string {
  if (check.kind === 'secret_store') {
    return '本地密钥只保存加密材料和引用信息，不在界面回显明文。';
  }

  if (check.kind === 'spend_guard') {
    return '单项目成本上限和重试次数会在后续真实调用前生效。';
  }

  if (check.kind === 'provider_sandbox') {
    return '当前只允许连接测试，真实生成、媒体读取和 FFmpeg 调用仍保持关闭。';
  }

  if (check.status === 'skipped') {
    return '该接口未启用，当前继续使用本地确定性生产链路。';
  }

  if (check.status === 'warning') {
    return '该接口已启用，但供应商、模型、接口地址或密钥仍需补齐。';
  }

  if (check.status === 'ok') {
    return '连接测试所需的本地配置已齐备，真实生成调用仍保持关闭。';
  }

  return check.message;
}

function formatProviderStatus(status: string): string {
  return providerStatusLabels[status] ?? status;
}

const emptyPreflight: ProviderSettingsPreflight = {
  healthy: true,
  checks: [],
  recommendations: [],
};

const emptySafety: ProviderSafetyStatus = {
  stage: 'stage22_provider_safety_preflight',
  mode: 'placeholder_only',
  secretStore: {
    mode: 'stage22_metadata_only',
    storageScope: 'project_local_metadata',
    metadataStoreAvailable: true,
    rawSecretPersistenceAllowed: false,
    providerCallSecretsAvailable: false,
    checks: [],
  },
  spendGuard: {
    enabled: true,
    enforcementMode: 'hard_preflight_placeholder_block',
    projectCostCapCny: 50,
    retryLimit: 1,
    blocksProviderCalls: true,
    blocksWhenCapExceeded: true,
  },
  sandbox: {
    mode: 'stage22_no_provider_calls',
    providerCallsAllowed: false,
    externalNetworkAllowed: false,
    mediaReadAllowed: false,
    ffmpegAllowed: false,
    allowedAdapterKinds: [],
    outputContract: [],
  },
  blockingReasons: [],
};
