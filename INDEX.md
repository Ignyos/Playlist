# Ignyos Design System Documentation Index

## 📚 Documentation Overview

Complete documentation for the Ignyos WPF Design System - a reusable theme library for all Ignyos applications.

---

## 🚀 Quick Start

**Get started in 3 steps:**

1. **Include the theme** in your app:
```xaml
<ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.xaml"/>
```

2. **Use component styles**:
```xaml
<Button Style="{StaticResource Button.Primary}" Content="Save"/>
<TextBlock Style="{StaticResource Text.Heading}" Text="Title"/>
```

3. **Reference design tokens**:
```xaml
<Border Background="{StaticResource Brush.Accent.Primary}"/>
```

**For complete setup instructions**, see [themes/Ignyos/README.md](themes/Ignyos/README.md)

---

## 📖 Documentation Files

### Theme Library Documentation

| Document | Purpose | For Whom |
|----------|---------|----------|
| [themes/Ignyos/README.md](themes/Ignyos/README.md) | **Theme library usage guide** | All developers using the theme |
| [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md) | **Quick reference** - Colors, typography, spacing tokens | Designers & developers |
| [THEME_LIBRARY_COMPLETE.md](THEME_LIBRARY_COMPLETE.md) | **Overview** - Architecture and structure | Tech leads & architects |
| [DESIGN_SYSTEM_IMPLEMENTATION.md](DESIGN_SYSTEM_IMPLEMENTATION.md) | **Implementation guide** - Details of theme creation | Maintenance & contributors |
| [COMPLETION_SUMMARY.md](COMPLETION_SUMMARY.md) | **Summary** - What was built and why | Project managers |

---

## 🎨 Design System Contents

### Color System
- **20+ color tokens** organized by category (Accent, Neutral, Semantic, Surface)
- **30+ brush aliases** for easy reference
- **2 gradient brushes** for modern effects
- Located in: `themes/Ignyos/Ignyos.Colors.xaml`

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#color-tokens)

### Typography System
- **5 font sizes** from 12pt to 30pt
- **6 text styles** (Display, Heading, SubHeading, Body, Body.Small, Muted)
- **Proper weights and line heights**
- Located in: `themes/Ignyos/Ignyos.Tokens.xaml`

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#typography)

### Spacing System
- **6 spacing values** from 8px to 64px (8px grid)
- Named tokens: Space.2XS, Space.XS, Space.SM, Space.MD, Space.LG, Space.XL
- Located in: `themes/Ignyos/Ignyos.Tokens.xaml`

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#spacing-scale)

### Visual Effects
- **Border radius**: 8px (MD), 16px (LG)
- **Drop shadows**: 40px blur, 4px depth
- **Gradient brushes**: Primary accent, header elevation
- Located in: `themes/Ignyos/Ignyos.Colors.xaml` and `Ignyos.Tokens.xaml`

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#visual-effects)

### Component Styles
- **Button variants**: Primary, Secondary, Ghost
- **Menu components**: Menu.Base, MenuItem.Base
- All with proper states: Hover, Active, Disabled
- Located in: `themes/Ignyos/Ignyos.Button.xaml` and `Ignyos.Menu.xaml`

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#component-styles)

---

## 📁 File Structure

```
d:\GitHub\Ignyos\Playlist\
│
├── themes/Ignyos/                           ← Theme Library
│   ├── Ignyos.xaml                         (Master entry point)
│   ├── Ignyos.Colors.xaml                  (Color tokens & brushes)
│   ├── Ignyos.Tokens.xaml                  (Typography, spacing, effects)
│   ├── Ignyos.Button.xaml                  (Button components)
│   ├── Ignyos.Menu.xaml                    (Menu components)
│   └── README.md                           (Theme library usage)
│
├── src/Playlist/                            ← Application
│   ├── MainWindow.xaml                     (Uses theme via pack:// URI)
│   ├── Styles/
│   │   └── StyleGuide.Proposed.xaml        (Original, can be removed)
│   └── ...
│
└── Documentation/                           ← This project
    ├── DESIGN_SYSTEM_REFERENCE.md          (Quick reference)
    ├── THEME_LIBRARY_COMPLETE.md           (Architecture overview)
    ├── DESIGN_SYSTEM_IMPLEMENTATION.md     (Implementation details)
    ├── COMPLETION_SUMMARY.md               (What was built)
    └── INDEX.md                            (This file)
```

---

## 🎯 Key Concepts

### Modular Architecture
The theme is organized in layers with clear dependencies:

```
Layer 1: Colors (Base)
    ↓ No dependencies
    
Layer 2: Tokens (Foundation)
    ↓ Depends on: Colors
    
Layer 3: Components
    ↓ Depend on: Colors + Tokens
    
Master: Ignyos.xaml
    ↓ Merges all layers
```

**→ See** [THEME_LIBRARY_COMPLETE.md](THEME_LIBRARY_COMPLETE.md#architecture-overview) for details

### Design System Benefits

✅ **Single Source of Truth** - Define colors, typography once, use everywhere
✅ **Visual Consistency** - All Ignyos apps use identical design
✅ **Reduced Duplication** - No need to recreate styles in each app
✅ **Easy Maintenance** - Update colors in one place, affects all apps
✅ **Modularity** - Use colors only, or full component library
✅ **Professional Documentation** - Reference guide included

**→ See** [DESIGN_SYSTEM_IMPLEMENTATION.md](DESIGN_SYSTEM_IMPLEMENTATION.md#design-system-benefits)

---

## 💻 Usage Patterns

### Pattern 1: Full Theme (Recommended)
```xaml
<ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.xaml"/>
```
Includes colors, typography, spacing, buttons, menus.

### Pattern 2: Colors Only
```xaml
<ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.Colors.xaml"/>
```
Just color and brush tokens, minimal footprint.

### Pattern 3: Colors + Tokens
```xaml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.Colors.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.Tokens.xaml"/>
</ResourceDictionary.MergedDictionaries>
```
Colors and typography/spacing, no component styles.

### Pattern 4: Custom Overrides
```xaml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/themes/Ignyos/Ignyos.xaml"/>
    <ResourceDictionary Source="Styles/MyAppCustom.xaml"/>
</ResourceDictionary.MergedDictionaries>
```
Use Ignyos theme as base, override specific styles.

**→ See** [themes/Ignyos/README.md](themes/Ignyos/README.md#quick-start) for complete examples

---

## 🔍 How to Find What You Need

### I want to...

**Use the theme in my app**
→ [themes/Ignyos/README.md](themes/Ignyos/README.md) - Quick Start section

**See all available colors**
→ [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#color-tokens)

**See all typography options**
→ [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#typography)

**Use a button component**
→ [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#button-primary) - Components section

**Create a styled panel/card**
→ [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#common-tasks) - Common Tasks

**Understand the architecture**
→ [THEME_LIBRARY_COMPLETE.md](THEME_LIBRARY_COMPLETE.md#architecture-overview)

**Learn implementation details**
→ [DESIGN_SYSTEM_IMPLEMENTATION.md](DESIGN_SYSTEM_IMPLEMENTATION.md)

**See color/spacing/typography values**
→ [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md) - Quick Reference section

**Customize the theme**
→ [themes/Ignyos/README.md](themes/Ignyos/README.md#customization)

---

## 🛠️ Theme Files

### Core Files

| File | Lines | Purpose |
|------|-------|---------|
| Ignyos.xaml | 45 | Master entry point |
| Ignyos.Colors.xaml | 80 | Colors & brushes |
| Ignyos.Tokens.xaml | 140 | Typography & tokens |
| Ignyos.Button.xaml | 110 | Button styles |
| Ignyos.Menu.xaml | 55 | Menu styles |
| **Total XAML** | **430** | **Core theme system** |

---

## 📊 Color Palette Quick View

### Accent (Brand Color)
| Token | Value | Usage |
|-------|-------|-------|
| Primary | #C18B5A | Main actions |
| Strong | #FF9B6B | Hover states |

### Neutral (Dark Theme)
| Token | Value | Usage |
|-------|-------|-------|
| Background | #0A0A0A | Page background |
| Surface | #0F0F0F | Cards, panels |
| Text | #E0E0E0 | Regular text |
| Text.Strong | #FFFFFF | Bold text |

### Semantic
| Token | Value | Usage |
|-------|-------|-------|
| Success | #22C55E | Success messages |
| Warning | #F59E0B | Warnings |
| Danger | #EF4444 | Errors |
| Info | #3B82F6 | Info messages |

**→ See** [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md#color-tokens) for complete palette

---

## 🎓 Design Principles

The theme embodies these principles:

1. **Dark First** - Built for accessibility and reduced eye strain
2. **Warm Accent** - Orange (#C18B5A) as primary brand color
3. **Consistent Spacing** - 8px grid ensures visual rhythm
4. **Modern Interactions** - Shadows and rounded corners for depth
5. **Accessibility** - High contrast, clear focus states
6. **Reusability** - Shared across all Ignyos applications

**→ See** [themes/Ignyos/README.md](themes/Ignyos/README.md#design-principles)

---

## ✅ Status

**Build Status**: ✅ Success - `Build succeeded` with no errors

**Documentation**: ✅ Complete - All design tokens documented

**Implementation**: ✅ Complete - Playlist app successfully updated

**Testing**: ✅ Verified - Theme renders correctly in application

---

## 📞 Support

For questions about:
- **Theme usage**: See [themes/Ignyos/README.md](themes/Ignyos/README.md)
- **Available colors/tokens**: See [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md)
- **Implementation details**: See [DESIGN_SYSTEM_IMPLEMENTATION.md](DESIGN_SYSTEM_IMPLEMENTATION.md)
- **Architecture**: See [THEME_LIBRARY_COMPLETE.md](THEME_LIBRARY_COMPLETE.md)

---

## 📝 Version

**Ignyos Design System v1.0.0**

Based on design tokens from [styles.ignyos.com](https://styles.ignyos.com)
Last updated: January 2026

---

## Quick Navigation

| Purpose | Document |
|---------|----------|
| Getting Started | [themes/Ignyos/README.md](themes/Ignyos/README.md) |
| Quick Reference | [DESIGN_SYSTEM_REFERENCE.md](DESIGN_SYSTEM_REFERENCE.md) |
| Architecture | [THEME_LIBRARY_COMPLETE.md](THEME_LIBRARY_COMPLETE.md) |
| Implementation | [DESIGN_SYSTEM_IMPLEMENTATION.md](DESIGN_SYSTEM_IMPLEMENTATION.md) |
| Summary | [COMPLETION_SUMMARY.md](COMPLETION_SUMMARY.md) |

---

**Ready to use? Start with [themes/Ignyos/README.md](themes/Ignyos/README.md)!**
