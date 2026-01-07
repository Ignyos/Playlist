## Contributing to Playlist

Thank you for considering contributing to Playlist! This document provides guidelines for contributing to the project.

### Code of Conduct

Be respectful, professional, and constructive in all interactions.

### How Can I Contribute?

#### Reporting Bugs

Before creating bug reports, please check existing issues. When creating a bug report, include:

- Clear, descriptive title
- Detailed steps to reproduce
- Expected vs actual behavior
- Screenshots if applicable
- Environment details (Windows version, .NET version)
- Error messages or logs

#### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- Clear, descriptive title
- Detailed description of the proposed functionality
- Why this enhancement would be useful
- Examples of how it would be used

#### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Commit with clear messages (`git commit -m 'Add some AmazingFeature'`)
5. Push to your branch (`git push origin feature/AmazingFeature`)
6. Open a Pull Request

### Development Setup

#### Prerequisites

- Visual Studio 2022 or VS Code
- .NET 9.0 SDK
- Git

#### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/Playlist.git
cd Playlist

# Add upstream remote
git remote add upstream https://github.com/Ignyos/Playlist.git

# Create a branch
git checkout -b feature/my-feature

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
cd src/Playlist
dotnet run
```

#### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release

# Clean
dotnet clean
```

#### Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/Playlist

# Update database
dotnet ef database update --project src/Playlist

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/Playlist
```

### Coding Standards

#### C# Style Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for I/O operations
- Handle exceptions appropriately

#### Code Organization

```
src/Playlist/
├── Data/           # DbContext and database configuration
├── Models/         # Entity models
├── Services/       # Business logic
├── Views/          # XAML windows and controls
├── ViewModels/     # View models (if using MVVM)
└── Migrations/     # EF Core migrations
```

#### XAML Guidelines

- Use descriptive `x:Name` for controls
- Follow WPF best practices
- Keep code-behind minimal
- Use data binding where appropriate
- Comment complex layouts

#### Git Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit first line to 72 characters
- Reference issues and pull requests after the first line

Examples:
```
Add full-screen mode to media player

Implement F11 keyboard shortcut and overlay controls
that auto-hide after 3 seconds of inactivity.

Fixes #123
```

### Testing

Before submitting a pull request:

1. Test your changes thoroughly
2. Ensure the application builds without errors
3. Test on a clean installation if possible
4. Verify no regressions in existing functionality

### Documentation

- Update README.md if adding new features
- Add XML comments for public APIs
- Update CHANGELOG.md following [Keep a Changelog](https://keepachangelog.com/) format
- Update GitHub Pages docs if needed

### Questions?

Feel free to open an issue for questions or clarifications.

### License

By contributing, you agree that your contributions will be licensed under the MIT License.
