import { KeyRound, Power, RefreshCcw, Save, ShieldCheck, SlidersHorizontal, Trash2 } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import {
  getProviderSettings,
  getProviderSettingsPreflight,
  updateProviderSettings,
} from '../../shared/api/controlPlaneClient';
import type {
  ProviderAdapterSettings,
  ProviderCostGuardrails,
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
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [message, setMessage] = useState('正在读取本地 provider 配置');

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
      setMessage('本地 provider 配置已同步');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '读取 provider 配置失败');
    } finally {
      setLoadState('idle');
    }
  };

  const refreshPreflight = async () => {
    setLoadState('loading');
    try {
      const nextPreflight = await getProviderSettingsPreflight();
      setSettings((current) => (current ? { ...current, preflight: nextPreflight } : current));
      setMessage(nextPreflight.healthy ? 'Provider preflight 通过' : 'Provider preflight 需要处理');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Provider preflight 失败');
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
          enabled: adapter.enabled,
          apiKey: apiKeyInputs[adapter.kind]?.trim() || null,
          clearApiKey: Boolean(clearApiKeys[adapter.kind]),
        })),
      });
      applySettings(nextSettings);
      setMessage('Provider 配置已保存，真实调用仍保持禁用');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '保存 provider 配置失败');
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

  return (
    <section className="provider-settings-view">
      <div className="provider-settings-heading">
        <div>
          <p className="eyebrow">Stage 22</p>
          <h1>Provider 前配置</h1>
        </div>
        <div className="provider-settings-actions">
          <span className={preflight.healthy ? 'api-chip connected' : 'api-chip'}>
            {preflight.healthy ? '占位预检通过' : '占位预检待处理'}
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
          <span>已启用 adapter</span>
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
          <span>真实 provider 调用</span>
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
                    <p>{adapter.capabilityFlags.join(' / ')}</p>
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
                    ? '待保存新 key'
                    : adapter.apiKeyConfigured
                      ? `已配置 ${adapter.apiKeyPreview}`
                      : '未配置 key'}
                </span>
                <small>{adapter.safety.secretStoreMode}</small>
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
                label="Secret Store"
                status={safety.secretStore.rawSecretPersistenceAllowed ? 'error' : 'ok'}
                value={safety.secretStore.mode}
              />
              <SafetyRow
                label="Spend Guard"
                status={safety.spendGuard.enabled ? 'ok' : 'error'}
                value={safety.spendGuard.enforcementMode}
              />
              <SafetyRow
                label="Sandbox"
                status={safety.sandbox.providerCallsAllowed ? 'error' : 'ok'}
                value={safety.sandbox.mode}
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
                    <strong>{check.label}</strong>
                    <p>{statusLabel(check.status)}</p>
                  </div>
                  <span className={`status-pill ${check.status}`}>{check.status}</span>
                </article>
              ))}
            </div>
          </section>
        </aside>
      </div>
    </section>
  );
}

function SafetyRow({ label, status, value }: { label: string; status: string; value: string }) {
  return (
    <article className="provider-safety-row">
      <div>
        <strong>{label}</strong>
        <p>{value}</p>
      </div>
      <span className={`status-pill ${status}`}>{status}</span>
    </article>
  );
}

function statusLabel(status: string): string {
  if (status === 'ok') {
    return '占位配置完整';
  }

  if (status === 'warning') {
    return '已启用但配置不完整';
  }

  if (status === 'skipped') {
    return '当前未启用';
  }

  return '需要处理';
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
