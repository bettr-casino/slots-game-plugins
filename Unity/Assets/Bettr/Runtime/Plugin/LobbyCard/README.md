# Lobby Card Prefab Plugin

### About Plugins

Bettr client plugins are modular and extensible packages that are loaded at runtime. They are used to extend the functionality of the Bettr client.
Bettr client plugins packaged similar to asset bundles and are loaded using the AssetBundle loading API. The difference is that Bettr client plugins are packaged as independent fully functional modules that can be loaded and unloaded at runtime.

## Overview

The Lobby Card Prefab Bettr Plugin provides a plugin package that represents a fully functional Lobby Card Prefab used to build a themed lobby.

Consider a dynamic lobby that is defined as follows:

```json
{
    "lobby": {
        "name": "Slots Lobby",
        "cards": [
            {
                "name": "Slots Card",
                "prefab": "slots-card",
                "data": {
                    "jackpot": "1000000",
                    "jackpotAnimation": "jackpot-animation"
                }
            }
        ]
    }
}
```

## Installation