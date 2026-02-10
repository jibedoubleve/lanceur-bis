## PR Review: (#1140) Add access to CLR from script and update editor

### Overview
This PR adds CLR (.NET) interop support to Lua scripts, allowing users to import and use .NET types directly from their Lua code. It also replaces the old modal-based Lua code editor (`CodeEditorControl`) with a full-page editor (`LuaEditorView`) backed by a proper ViewModel, adds script output/logging capabilities, updates documentation with CLR usage examples and a new script database page, and changes the application hotkey.

### Issues

1. **`ConfirmDiscardOrSaveAsync` always returns `true`, making the confirmation dialog pointless**
   The method asks the user "Do you want to save before leaving?" but always returns `true` regardless of the answer. If the user is shown a dialog, there should be an option to *cancel* navigation (e.g., if they click "No" or dismiss the dialog, they may want to stay). Currently, clicking "No" silently discards changes with no way to cancel.
   [LuaEditorViewModel.cs (lines 113-128)](https://github.com/jibedoubleve/lanceur-bis/pull/1214/files#diff-LuaEditorViewModel.cs)

2. **Hotkey changed from `Win+Ctrl+R` to `Win+Alt+P` without clear justification**
   This is a breaking change for existing users. `Win+Alt` combinations can also conflict with system or other application shortcuts. If intentional, this should be documented in release notes.
   [App.xaml.cs (line 224)](https://github.com/jibedoubleve/lanceur-bis/pull/1214/files#diff-App.xaml.cs)

3. **`MemoryInfrastructureSettingsProvider` now hardcodes a debug-specific path**
   The DB path was changed to `Desktop\lanceur\debug.sqlite`. This looks like a leftover debug change that shouldn't be merged to master, as it would affect the default settings path for non-debug scenarios.
   [MemoryInfrastructureSettingsProvider.cs (lines 16-17)](https://github.com/jibedoubleve/lanceur-bis/pull/1214/files#diff-MemoryInfrastructureSettingsProvider.cs)

4. **`LogTrace` used for script output on error ΓÇö should be `LogWarning` or `LogError`**
   In `LuaManager.ExecuteScript`, when an exception is caught, the output is logged at `Trace` level. Script execution failures are significant events that warrant at least `Warning` level so they appear in typical log configurations.
   [LuaManager.cs (line 72)](https://github.com/jibedoubleve/lanceur-bis/pull/1214/files#diff-LuaManager.cs)

5. **Missing newlines at end of several new files**
   `LuaScriptExtensions.cs`, `LuaScriptOutput.cs`, and `NotificationScriptAdapter.cs` all lack a trailing newline, which can cause diff noise and warnings from some tools.

### Suggestions

1. **`LuaScriptOutput` XML doc says "logging to a file on the desktop"** but the class actually logs to an in-memory `StringBuilder`. The summary should be updated to match the actual behavior.
   [LuaScriptOutput.cs (line 7)](https://github.com/jibedoubleve/lanceur-bis/pull/1214/files#diff-LuaScriptOutput.cs)

2. **Consider using `IFormatProvider` with `DateTime.Now.ToString("o")`** to avoid locale-dependent edge cases in the ISO 8601 output. Using `CultureInfo.InvariantCulture` is a common best practice.

3. **The `LuaEditorView` singleton registers a `PropertyChanged` handler on the ViewModel but never unsubscribes.** Since both are singletons this is not a leak per se, but if the lifecycle ever changes, this could become one. A comment noting the intentional coupling would help future maintainers.

4. **The doc page `4.1.lua-script-db.md` uses a relative link `[Lua editor](pages/usermanual/4.lua-scripting.md)`** ΓÇö verify this resolves correctly in Docsify since it's not prefixed with `/` or `../`.

### What Looks Good

- **Clean MVVM refactoring**: Moving from the code-behind `CodeEditorControl` to a proper `LuaEditorViewModel` with `ObservableProperty` and `RelayCommand` is a significant architectural improvement. The separation of concerns is well done.
- **Fluent builder pattern for Lua setup**: `LuaScriptExtensions` using C# 14 extension types with method chaining (`SetEncoding().AddClrPackage().AddContext()...`) makes the Lua initialization readable and composable.
- **Good test coverage**: Both test files were updated to match the new `LuaManager` constructor signature, showing the tests are kept in sync.
- **Thorough documentation**: The new script database page with real-world examples (environment routing, auto-detect latest version) and the CLR usage guide with nested type explanation are excellent for end users.
- **Output/Logs tabs in the editor**: Adding structured output and log capture for dry runs is a meaningful UX improvement for script debugging.
