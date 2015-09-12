# UserAgentReport

## Building Dependencies

### Current Versions

- SQLite: 3.8.11.1
- System.Data.SQLite: 1.0.98.1
- EntityFramework: 6.1.3

### Requirements

- Visual Studio 2012 or later with native tools installed.

### SQLite (native DLL)

This is the native DLL that is the whole SQLite library.

1. Make sure you have the native build tools for Visual Studio.
2. Download the source amalgamation ZIP file from the project's [download page](https://www.sqlite.org/download.html).
3. Extract the ZIP file to `SOURCE_DIR`.
4. Make the directories `SOURCE_DIR\x86` and `SOURCE_DIR\x64`.
5. Open the x86 Native Tools Command Prompt.
6. Nativate to `SOURCE_DIR\x86`.
7. Execute: `cl ../sqlite3.c /DSQLITE_API=__declspec(dllexport) /O2 /link /dll /out:sqlite3.dll`
8. Open the x64 Native Tools Command Prompt.
9. Nativate to `SOURCE_DIR\x64`.
10. Execute step 7.

The x86 native DLL is at `SOURCE_DIR\x86\sqlite3.dll`. The x64 native DLL is in `SOURCE_DIR\x64\sqlite3.dll`.

### System.Data.SQLite (managed DLL)

This is the managed assembly that allows C# to call into the native SQLite library.

1. Download the latest source code zip file from the project's [download page](https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki).
2. Extract the source code ZIP to `SOURCE_DIR`.
3. Open `SOURCE_DIR\readme.htm` and find out what the matching EntityFramework version is.
4. Download the corresponding EntityFramework NuGet package: `https://www.nuget.org/api/v2/package/EntityFramework/VERSION`
5. Extract the NuGet package to `SOURCE_DIR\Externals\EntityFramework` so that `SOURCE_DIR\Externals\EntityFramework\lib\net45\EntityFramework.dll` is a valid file path.
6. Open to `SOURCE_DIR\Setup\build_mono.bat` in a text editor.
7. Change `SET YEARS=2008 2013` to `SET YEARS=2012`
8. Open the `Developer Command Prompt`.
9. Navigate to `SOURCE_DIR\Setup`
10. Execute `build_mono.bat`. *Note: Mono is not required*.

The managed DLL is at `SOURCE_DIR\bin\2012\Release\bin\System.Data.SQLite.dll`.
