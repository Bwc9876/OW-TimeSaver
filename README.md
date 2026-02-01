# Time Saver

A mod that saves uh... time.

## Features

All of these are disabled by default, enable them by going to `Mods` -> `Time Saver`.

- Skip the splash screen
- Force Slate to shut up
- Skip the death sequence
- Always start with your suit on
- Skip startup popups (even ones from QSB and NH)
- Skip credits
- Skip post-credits scene

## API

If your mod would like to force disable any of these features you can use Time Saver's API. 

See the [API Guide](https://owml.outerwildsmods.com/guides/apis.html#consuming-apis) from the OWML docs for adding it. You can copy it from `TimeSaver/ITimeSaver.cs` (and change the namespace).

Each method of the API corresponds to a setting value. 

- `true` force enables
- `false` force disables
- `null` resets to user preference

