# 中文测试剧本 Fixtures

本目录用于保存后续人工测试和自动化测试可复用的中文剧本文本。

## 牡丹亭

- 完整文本：`mudan_ting_gutenberg_23849.txt`
- 测试输入样本：`mudan_ting_stage_input_zh.txt`
- 来源：Project Gutenberg eBook #23849
- 页面：https://www.gutenberg.org/ebooks/23849
- UTF-8 文本：https://www.gutenberg.org/ebooks/23849.txt.utf-8
- 作者：汤显祖
- 页面标注：Public domain in the USA

说明：

- `mudan_ting_gutenberg_23849.txt` 是下载的完整公版文本，保留 Project Gutenberg 头尾信息。
- `mudan_ting_stage_input_zh.txt` 是便于当前 Web 输入框测试的现代化短剧样本，控制在 500 到 2000 字之间，基于《牡丹亭》公版故事整理，不用于代表原文。
- 当前 deterministic skills 不接真实模型，也不生成真实媒体文件；这些文本只用于验证输入保存、Control API、Worker、checkpoint 和结果卡展示链路。
