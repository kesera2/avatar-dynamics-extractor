# Avatar Dynamics Extractor - Unit Tests

Unity Test Runner用の単体テストスイート（元コード無変更）

## 📁 テスト構造

```
Tests/
├── Editor/
│   ├── Tests.asmdef           # 最小限のアセンブリ定義
│   ├── SettingsTests.cs       # Settings定数テスト
│   ├── UtilityTests.cs        # Utilityメソッドテスト  
│   ├── LocalizationTests.cs   # 多言語化基本テスト
│   ├── ServiceBasicTests.cs   # Service基本機能テスト
│   └── README.md              # このファイル
└── README.md
```

## 🧪 テスト実行方法

### Unity Test Runner (Edit Mode)
1. Unity Editorを開く
2. **Window > General > Test Runner**
3. **EditMode**タブを選択
4. **Run All**で全テスト実行

## 🎯 テストカテゴリ

### 1. **SettingsTests.cs** - 設定値テスト
- ✅ 全定数値の正確性検証
- ✅ GUI設定オプションの有効性
- ✅ ウィンドウ設定値
- ✅ ロゴパス生成

### 2. **UtilityTests.cs** - ユーティリティテスト  
- ✅ `hasParentGameObject`メソッド
- ✅ 階層関係判定の正確性
- ✅ エッジケース処理
- ✅ null入力処理

### 3. **LocalizationTests.cs** - 多言語化テスト
- ✅ サポート言語定義
- ✅ 表示名マッピング
- ✅ 言語ロード機能
- ✅ フォールバック処理

### 4. **ServiceBasicTests.cs** - サービス基本テスト
- ✅ インスタンス化
- ✅ デフォルト値設定
- ✅ プロパティ設定・取得
- ✅ 実行可能性判定
- ✅ コンポーネントカウント

## 🎭 テスト特徴

### **非侵入的設計**
- ✅ **元コード変更なし**: 既存コードを一切変更しない
- ✅ **Public API専用**: 公開インターフェースのみテスト
- ✅ **最小依存**: VRChatSDK依存を極力回避

### **実用性重視**
- ✅ **高速実行**: 複雑な依存関係を排除
- ✅ **安定性**: 外部要因に影響されにくい
- ✅ **完全分離**: 各テストが独立実行可能

## 🚀 カバー範囲

| クラス | カバー率 | 内容 |
|--------|----------|------|
| Settings | 100% | 全定数・メソッド |
| Utility | 100% | 全公開メソッド |
| Localization | 80% | 基本機能・構造 |
| AvatarDynamicsExtractorService | 60% | VRC無依存部分 |

## 📋 今後の拡張

VRChatSDK環境での統合テスト:
- 実際のコンポーネント抽出テスト
- UI統合テスト  
- エラーハンドリングテスト
- パフォーマンステスト

## 💡 実行要件

- Unity 2022.3以上
- Unity Test Framework
- NUnit Framework  
- Avatar Dynamics Extractor Package

## 🔍 トラブルシューティング

**テストが見つからない場合:**
1. Assembly Definition参照の確認
2. UNITY_INCLUDE_TESTS定義の有効化
3. Test Runnerの再読み込み

**コンパイルエラーの場合:**
1. VRChatSDKの正常インストール確認
2. アセンブリ参照の整合性チェック
3. Unity再起動