param(
  [string]$ProjectRoot = "D:\code\MiLuStudio"
)

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path

$env:TMP = Join-Path $resolvedRoot ".tmp"
$env:TEMP = Join-Path $resolvedRoot ".tmp"
$env:npm_config_cache = Join-Path $resolvedRoot ".cache\npm"
$env:PNPM_HOME = Join-Path $resolvedRoot ".cache\pnpm"
$env:COREPACK_HOME = Join-Path $resolvedRoot ".cache\corepack"
$env:YARN_CACHE_FOLDER = Join-Path $resolvedRoot ".cache\yarn"
$env:PIP_CACHE_DIR = Join-Path $resolvedRoot ".cache\pip"
$env:PIP_CONFIG_FILE = Join-Path $resolvedRoot ".config\pip\pip.ini"
$env:PYTHONUSERBASE = Join-Path $resolvedRoot ".python-userbase"
$env:NUGET_PACKAGES = Join-Path $resolvedRoot ".nuget\packages"
$env:DOTNET_CLI_HOME = Join-Path $resolvedRoot ".dotnet"
$env:ELECTRON_CACHE = Join-Path $resolvedRoot ".cache\electron"
$env:ELECTRON_BUILDER_CACHE = Join-Path $resolvedRoot ".cache\electron-builder"
$env:PLAYWRIGHT_BROWSERS_PATH = Join-Path $resolvedRoot ".ms-playwright"
$env:HF_HOME = Join-Path $resolvedRoot ".cache\huggingface"
$env:TRANSFORMERS_CACHE = Join-Path $resolvedRoot ".cache\huggingface\transformers"
$env:MILUSTUDIO_PYTHON = "D:\soft\program\Python\Python313\python.exe"
$env:ControlPlane__OcrTesseractPath = Join-Path $resolvedRoot "runtime\tesseract\tesseract.exe"
$pdfRasterizerCandidates = @(
  (Join-Path $resolvedRoot "runtime\poppler\Library\bin\pdftoppm.exe"),
  (Join-Path $resolvedRoot "runtime\poppler\bin\pdftoppm.exe")
)
$resolvedPdfRasterizer = $null
foreach ($candidate in $pdfRasterizerCandidates) {
  if (Test-Path -LiteralPath $candidate) {
    $resolvedPdfRasterizer = $candidate
    break
  }
}
if ([string]::IsNullOrWhiteSpace($resolvedPdfRasterizer)) {
  $resolvedPdfRasterizer = $pdfRasterizerCandidates[0]
}

$env:ControlPlane__PdfRasterizerPath = $resolvedPdfRasterizer
$env:ControlPlane__PdfRasterizerDpi = "180"
$env:ControlPlane__PdfRasterizerPageLimit = "3"

$ocrTessdataPath = Join-Path $resolvedRoot "runtime\tesseract\tessdata"
if (Test-Path -LiteralPath $ocrTessdataPath) {
  $env:ControlPlane__OcrTessdataPath = $ocrTessdataPath
} else {
  Remove-Item Env:\ControlPlane__OcrTessdataPath -ErrorAction SilentlyContinue
}

@(
  $env:TMP,
  $env:npm_config_cache,
  $env:PNPM_HOME,
  $env:COREPACK_HOME,
  $env:YARN_CACHE_FOLDER,
  $env:PIP_CACHE_DIR,
  (Split-Path -Parent $env:PIP_CONFIG_FILE),
  $env:PYTHONUSERBASE,
  $env:NUGET_PACKAGES,
  $env:DOTNET_CLI_HOME,
  $env:ELECTRON_CACHE,
  $env:ELECTRON_BUILDER_CACHE,
  $env:PLAYWRIGHT_BROWSERS_PATH,
  $env:HF_HOME,
  $env:TRANSFORMERS_CACHE
) | ForEach-Object {
  New-Item -ItemType Directory -Force -Path $_ | Out-Null
}

Write-Output "MiLuStudio environment configured for $resolvedRoot"
