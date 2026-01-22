# Fix Release Script
# This script will properly set up the v1.0.1 release and push it to GitHub

$ErrorActionPreference = "Stop"

Write-Host "=== Fixing v1.0.1 Release ===" -ForegroundColor Cyan
Write-Host ""

# 1. Stash current changes
Write-Host "Stashing current changes..." -ForegroundColor Cyan
git stash push -m "Release workflow fixes"

# 2. Checkout main branch
Write-Host "Switching to main branch..." -ForegroundColor Cyan
git checkout main

# 3. Cherry-pick the v1.0.1 commit
Write-Host "Cherry-picking v1.0.1 commit..." -ForegroundColor Cyan
git cherry-pick 3502e85

# 4. Apply stashed changes
Write-Host "Applying release workflow fixes..." -ForegroundColor Cyan
git stash pop

# 5. Create RELEASE_NOTES.txt from the rc_ file
Write-Host "Creating RELEASE_NOTES.txt..." -ForegroundColor Cyan
Copy-Item "rc_1.0.1_2026-01-11-18-40.txt" "RELEASE_NOTES.txt" -Force

# 6. Commit the workflow fixes and RELEASE_NOTES.txt
Write-Host "Committing workflow fixes..." -ForegroundColor Cyan
git add release.ps1 .github/workflows/release.yml RELEASE_NOTES.txt
git commit -m "Fix release workflow to use RELEASE_NOTES.txt for GitHub releases"

# 7. Delete and recreate the v1.0.1 tag
Write-Host "Recreating v1.0.1 tag..." -ForegroundColor Cyan
git tag -d v1.0.1
git tag -a v1.0.1 -m "Release 1.0.1"

# 8. Push everything
Write-Host ""
Write-Host "Ready to push to GitHub!" -ForegroundColor Yellow
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  - Push main branch with all changes" -ForegroundColor Yellow
Write-Host "  - Push v1.0.1 tag to trigger GitHub Actions" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Push to GitHub now? (y/[n])"

if ($confirm -match '^y(es)?$') {
    Write-Host ""
    Write-Host "Pushing to origin..." -ForegroundColor Cyan
    git push origin main
    git push origin v1.0.1
    
    Write-Host ""
    Write-Host "Done! GitHub Actions should now be triggered." -ForegroundColor Green
    Write-Host "Check: https://github.com/Ignyos/Playlist/actions" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "Push cancelled. You can push manually later with:" -ForegroundColor Yellow
    Write-Host "  git push origin main" -ForegroundColor White
    Write-Host "  git push origin v1.0.1" -ForegroundColor White
}

Write-Host ""
Write-Host "Release setup complete!" -ForegroundColor Green
