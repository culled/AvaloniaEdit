# Updated AvaloniaEdit

This is a port of [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) for [Avalonia](https://github.com/AvaloniaUI/Avalonia).

For a piece of software I've been creating, I choose to use and extend the AvaloniaEdit text editor because I needed rich text editor capabilities. In this fork, I've added functionality for:
  - Proper word-wrapping
  - Per-paragraph margins
  - Padding at the top and bottom of the text editor view
  - Zooming capabilities using a scale-transform
  - Ability to set the width of a document and center it in a larger viewport (like MS Word)
 
 Some, but not all parts have been properly updated for .NET 5 and Avalonia 10
 
 I know my implementations may not be the best and/or in-line with how the awesome guys at AvaloniaEdit have done stuff, but I've tried to not make hacks to get all this functionality to work.
 
 Feel free to use this if you need rich text editing functionality in your app!
