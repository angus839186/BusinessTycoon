# Game Data Rule

## 資料來源

`Assets/GameData/Source` 內的 TSV 是遊戲靜態資料的唯一來源。

- `Resources.tsv`：資源基本資料
- `Recipes.tsv`：資源生產配方
- `Buildings.tsv`：建築基本資料

ScriptableObject 是匯入結果。不要手動修改由 TSV 管理的欄位。

## 建議工作流程

1. 使用 Google 試算表編輯資料。
2. 每個分頁下載為 Tab-separated values（TSV）。
3. 用下載的檔案取代 `Assets/GameData/Source` 內對應檔案。
4. 開啟 Unity。
5. 執行 `Tools > Game Data Importer`。
6. 按 `Validate All`。
7. 修正所有 Error。
8. 按 `Import All`。
9. 檢查 Warning 與匯入結果。
10. 提交 TSV 與產生或更新的 `.asset`。

## 通用格式

- 第一列必須是標頭。
- 一列代表一筆資料。
- ID 必須唯一，建立後不要任意修改。
- ID 建議使用英文、數字與底線。
- Boolean 使用 `TRUE` 或 `FALSE`。
- 小數使用 `.`。
- 顏色使用 `#RRGGBB` 或 `#RRGGBBAA`。
- Unity 資產路徑必須從 `Assets/` 開始。
- 不要加入多餘的 Tab、合併儲存格或空白標頭。

## Resources.tsv

必要欄位：

`resourceId`, `displayName`, `basePrice`, `iconPath`, `enabled`

規則：

- `resourceId` 必須唯一。
- `displayName` 不可為空。
- `basePrice` 必須大於或等於 0。
- `iconPath` 留空時保留 SO 原有 Icon。
- `enabled` 控制資料是否啟用。

## Recipes.tsv

必要欄位：

`ownerResourceId`, `ioType`, `resourceId`, `amount`, `enabled`

規則：

- `ownerResourceId` 是擁有此配方的資源。
- `ioType` 只能是 `Input` 或 `Output`。
- `resourceId` 必須存在於 `Resources.tsv`。
- `amount` 必須大於 0。
- 同一個資源可以有多筆 Input 與 Output。
- 匯入時會重建該 Resource 的 Recipe 陣列。

## Buildings.tsv

必要欄位以目前檔案標頭為準。

規則：

- `buildingId` 必須唯一。
- `category` 使用 `Resource`, `Processing`, `Commercial`, `Storage`。
- Resource 與 Processing 必須設定 `producedResourceId`。
- Commercial 與 Storage 不可設定 `producedResourceId`。
- Storage 的 `acceptsAnyResource` 必須為 `TRUE`。
- 非 Storage 建築的生產時間必須大於 0。
- `workSpeedMultiplier` 必須大於 0。

## 匯入與刪除

匯入順序固定為：

`Resources -> Recipes -> Buildings`

TSV 移除資料列不會自動刪除既有 SO，而會顯示孤立資產 Warning。
確認沒有其他資產引用後，再從 Unity Project 視窗手動刪除該 SO。

TSV 沒有包含的 SO 欄位不會被覆蓋，例如目前未列入 TSV 的建造成本與維護成本。