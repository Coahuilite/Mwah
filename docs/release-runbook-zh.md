# 构建、出包与发布

本页提供操作顺序；当前授权看 TODO.md，已发布渠道事实看 [release-status.md](release-status.md)。本地提交及版本分支 push 沿用既有授权和机械隐私门；main 合入、tag、GitHub Release、Workshop 上传、远端／默认分支变更分别需要维护者授权。

## 日常候选

```powershell
pwsh scripts/verify-local.ps1
pwsh scripts/pack-dev.ps1 -Zip
pwsh scripts/privacy-audit.ps1 -PackageDir dist/dev/Mwah
```

- `verify-local` 检查双游戏目标 × Dev／Steam／GitHub 构建、XML、双语及跨根重复、Def／天意／精确三出口、加载声明与 DLL 框架／身份、版本跟随面、设置和分发隐私。保留原断言，复用仍适用于同一候选和输入的证据。
- `pack-dev` 必先构建，再暂存 `dist/dev/Mwah`；不带 `-Zip` 只生成文件夹，带它另产单一 Mwah 顶层归档。`version.txt` 记录产品版本、渠道和 commit，脏树标 dirty，只用作开发候选。
- `-PackageDir` 检查实际包，包括文本、DLL 可打印字符串及 PNG 元数据。工具不判断图像像素隐私；图像变更须目检。

完整双版本构建需要 .NET 10 SDK 和可用的 NuGet 还原／缓存。1.6 为 net472／引用 1.6.4871，1.7 为 net10.0／引用 1.7.4990-unstable；实测游戏 build 不自动更新编译引用。只构建可用 `pwsh scripts/build-runtime.ps1`，按需传 `-BuildFlavor` 或 `-GameVersions`。直接构建 csproj 默认仍是 1.6；脚本自带 restore，不接受 NoRestore 开关。

纯文档只检查语义、实际读取方、链接及隐私，不机械重跑未受影响构建。禁止新增单元测试、追加用例或等价换名脚本；选择能回答真实验收问题的现有检查和维护者实机路径。

## 包内只放运行内容

根 Defs／Languages 各一份；1.6、1.7 各一原生 DLL；About、LoadFolders、version 标签；VNPE 登记／专用语言位于独立条件根。Source、docs、archive、art、工具、缓存、游戏引用 DLL、PDB 和工坊发布标识不进包。

每个 LoadFolders 选择显式包含 `/`、当前 minor 和条件兼容项。选择根不等于复制整个仓库；暂存按白名单取内容，检查基础／VNPE 有效组合、重复内容及唯一 DLL。

## 安装与加载由维护者确认

完整替换 `Mods/Mwah`，不叠加覆盖、不混用 DLL，只启用一个 coahuilite.mwah；保留原存档与设置。未被自定义列表选中的旧 Common 不会自行加载，但有效根中的旧文件仍可能被读取，故不保留旧安装残片。

实施者不复制安装、不运行游戏、不删除游戏目录内容、不建立 junction／symlink。布局改变只重开受影响的新包加载确认，未改变的玩法报告继续有效；场景清单、构建成功或文件选择均不能单独证明实机通过。具体候选、覆盖和下一责任方在[候选确认](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md)中。

## 提交与版本分支 push

1. 使用 Conventional Commits，在当前 `<minor>.x` 分支工作；历史只追加，不 amend／rebase。main 仅供正式发布，已归档分支不继续开发。
2. 提交前核对目标范围，包含本次新增文件，不夹带本地原稿或临时产物；暂存后运行 `git diff --cached --check`。
3. 每个 clone 用 `pwsh scripts/install-hooks.ps1` 安装既有 pre-push hook。push 自动执行 `privacy-audit.ps1 -FullHistory`，失败拒推；CI 在全量 clone 上重跑。归档只要还在仓库，同样受文本、历史、身份和媒体隐私规则约束。

普通版本分支 push 无需手工重复发布仪式。一次性脚本、反编译与调试输出放系统临时目录，不进入版本控制。

## 正式发布顺序

1. **确定干净候选及渠道。** csproj `<Version>` 是主源，About modVersion、工坊目标／中英版本行和双语 changelog 置顶条目同步；玩法／文案采用和真实缺口已记录，维护者决定版本、渠道及公开文案。
2. **执行发布门。** 使用该候选有效的 verify-local 结果，执行 `pwsh scripts/privacy-audit.ps1 -FullHistory -PrePush`。PrePush 的干净树、main 和待推 tag 集合自检只适用于发布，不套给日常开发。
3. **获准后合入 main。** 通过 PR squash merge；确认合并树与获准版本分支一致。发布 tag 必须在 main 历史上，Release CI 会校验血统。
4. **获准后发布 GitHub。** 在发布提交打 `vX.Y.Z` tag 并推送。CI 核对版本和血统，跑既有门，使用 GitHub flavor 构建 `dist/github/Mwah-vX.Y.Z.zip`，审计实际包后创建 Release。手工出包入口为 `pwsh scripts/pack-github.ps1 -Version vX.Y.Z`，打包不等于发布。
5. **获准后交付 Steam。** 在干净树运行 `pwsh scripts/pack-steam.ps1`，得到 `dist/steam/Mwah`。维护者自行复制到上传副本，只在该副本保管 PublishedFileId，完成工坊上传及页面更新；不入仓库或分发包。中英页面稿维护在 steam-workshop-page.md。
6. **记录实际结果。** 更新 release-status.md 的渠道、候选身份与完成依据，更新双语 changelog 的实际发布时间和 TODO 下一动作。一个渠道的完成不证明另一个渠道完成，未确认就保留未确认。

GitHub／Steam 各自构建自己的 flavor，不把上一渠道 DLL 贴上新标签再发行。工作树审计、全历史审计、实际包和外部渠道状态各证明自己的层，不互相推定。

## 旧文档如何退役

先把当前仍需的规则、渠道证据、恢复入口和未尽责任迁入其现行来源，更新直接读者，再将被替代文件放入 docs/archive。归档不是运行／构建输入，也不是必读冷记忆；本项目由维护者手动删除退役资料，执行者不代删。删除归档不应使现行入口、协议或自动消费者失效。
