# Ignyos Design System - Usage Guide

## Overview

The Ignyos design system is a comprehensive dark theme for WPF applications. It provides:
- Color tokens and brush aliases
- Typography and spacing tokens
- Pre-styled components (Button, Menu)
- Gradient and effect definitions

## Installation in Your WPF Project

### Step 1: Copy Theme Files

Copy the following files to your WPF project's `Themes` directory:
- `Ignyos.xaml` (master dictionary)
- `Ignyos.Colors.xaml` (color tokens)
- `Ignyos.Tokens.xaml` (typography, spacing, etc.)
- `Ignyos.Button.xaml` (button styles)
- `Ignyos.Menu.xaml` (menu styles)

**Directory structure:**
```
YourProject/
├── Themes/
│   ├── Ignyos.xaml
│   ├── Ignyos.Colors.xaml
│   ├── Ignyos.Tokens.xaml
│   ├── Ignyos.Button.xaml
│   └── Ignyos.Menu.xaml
├── App.xaml
└── MainWindow.xaml
```

### Step 2: Update Your .csproj File

Add the following `<ItemGroup>` section to your project file to ensure the theme XAML files are included as resources:

```xml
<ItemGroup>
  <!-- Ensure XAML theme files are included as resources -->
  <Resource Include="Themes\Ignyos.xaml" />
  <Resource Include="Themes\Ignyos.Colors.xaml" />
  <Resource Include="Themes\Ignyos.Tokens.xaml" />
  <Resource Include="Themes\Ignyos.Button.xaml" />
  <Resource Include="Themes\Ignyos.Menu.xaml" />
</ItemGroup>
```

**Important:** Without this step, the theme files won't be included in the compiled assembly, and runtime XAML parsing will fail with resource not found errors.

### Step 3: Reference the Theme in Your App

Add the theme to your `App.xaml` (recommended) or individual window XAML files:

**Option A: Global theme (App.xaml)**
```xaml
<Application x:Class="YourNamespace.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/Themes/Ignyos.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Option B: Window-specific theme (MainWindow.xaml)**
```xaml
<Window x:Class="YourNamespace.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/Themes/Ignyos.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>
    <!-- Rest of your window content -->
</Window>
```

## Using Theme Resources

Once the theme is referenced, you can use any defined resources in your XAML:

### Colors
```xaml
<Rectangle Fill="{StaticResource Brush.Accent.Primary}"/>
<TextBlock Foreground="{StaticResource Brush.Neutral.Text}"/>
```

### Component Styles
```xaml
<Menu Style="{StaticResource Menu.Base}">
    <MenuItem Header="File" Style="{StaticResource MenuItem.Base}"/>
</Menu>

<Button Style="{StaticResource Button.Primary}">Click Me</Button>
```

### Typography
```xaml
<TextBlock Style="{StaticResource Text.Heading1}"/>
<TextBlock Style="{StaticResource Text.Body}"/>
<TextBlock Style="{StaticResource Text.Caption}"/>
```

### Spacing & Effects
```xaml
<Border CornerRadius="{StaticResource Radius.Medium}"/>
<Border Effect="{StaticResource Shadow.Elevation2}"/>
```

## Available Resources

### Brushes
- **Accent Colors:** `Brush.Accent.Primary`, `Brush.Accent.Strong`
- **Neutral Surfaces:** `Brush.Neutral.Background`, `Brush.Neutral.Surface`, `Brush.Neutral.Surface.Hover`, `Brush.Neutral.Surface.Active`
- **Text Colors:** `Brush.Neutral.Text`, `Brush.Neutral.Text.Strong`, `Brush.Neutral.Text.Muted`
- **Semantic:** `Brush.Semantic.Success`, `Brush.Semantic.Warning`, `Brush.Semantic.Danger`, `Brush.Semantic.Info`

### Typography
- **Font Sizes:** `FontSize.Caption`, `FontSize.Body`, `FontSize.Subtitle`, `FontSize.Heading3`, `FontSize.Heading2`, `FontSize.Heading1`
- **Text Styles:** `Text.Caption`, `Text.Body`, `Text.Subtitle`, `Text.Heading3`, `Text.Heading2`, `Text.Heading1`

### Spacing
- **Padding/Margin:** `Spacing.XSmall` (2px), `Spacing.Small` (4px), `Spacing.Medium` (8px), `Spacing.Large` (12px), `Spacing.XLarge` (16px), `Spacing.XXLarge` (24px)

### Effects
- **Shadows:** `Shadow.Elevation1`, `Shadow.Elevation2`, `Shadow.Elevation3`
- **Border Radius:** `Radius.Small`, `Radius.Medium`, `Radius.Large`

## Troubleshooting

### XAML Parse Exception: "Resource not found"
**Cause:** Theme files not included in the compiled assembly
**Solution:** Ensure the `<ItemGroup>` with `<Resource>` entries is added to your .csproj file

### Resources appear undefined at design time
**Cause:** AssemblyName mismatch in pack URIs
**Solution:** Verify your pack URIs match this format: `pack://application:,,,/Themes/Filename.xaml`

### Styles not applying at runtime
**Cause:** Theme merged dictionary not loaded before controls are created
**Solution:** Ensure the theme is referenced in App.xaml before any other resources, or add it directly to each window's resources

## Customization

To customize the theme, edit the individual XAML files:
- **Colors:** Modify values in `Ignyos.Colors.xaml`
- **Typography:** Adjust font sizes and families in `Ignyos.Tokens.xaml`
- **Component Styles:** Edit specific component files (Button, Menu, etc.)

All changes will automatically apply to your application once you rebuild.
