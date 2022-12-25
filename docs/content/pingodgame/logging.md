---
title: "Logging Games"
date: 2017-10-17T15:26:15Z
draft: false
weight: 45
---

Game logging

```

void LogDebug(params object[] what);

void LogInfo(params object[] what);
	
void LogWarning(string message = null, params object[] what);

void LogError(string message = null, params object[] what);

PinGodLogLevel LogLevel { get; }
```

For example:

`pinGod.LogInfo("my message");`

## Log location

In the game name appdata directory. Use the `open_app_data.bat` to open directory instead of looking for it.