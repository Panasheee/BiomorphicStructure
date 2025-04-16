# PowerShell script to merge unrelated Git branch histories
# Created to fix BiomorphicStructure repository

Write-Host "Starting merge process for unrelated Git histories..." -ForegroundColor Green

# Step 1: Make sure we're up to date
Write-Host "Step 1: Updating local repository..." -ForegroundColor Yellow
git fetch origin
Write-Host "✅ Fetched latest changes" -ForegroundColor Green

# Step 2: Make sure we're on main branch
Write-Host "Step 2: Switching to main branch..." -ForegroundColor Yellow
git checkout main
Write-Host "✅ Now on main branch" -ForegroundColor Green

# Step 3: First check out the remote master branch locally
Write-Host "Step 3: Creating local master branch from remote..." -ForegroundColor Yellow
git checkout -b temp_master origin/master
Write-Host "✅ Created local master branch as temp_master" -ForegroundColor Green

# Step 4: Go back to main
Write-Host "Step 4: Switching back to main branch..." -ForegroundColor Yellow
git checkout main
Write-Host "✅ Now on main branch" -ForegroundColor Green

# Step 5: Merge master into main with special flag
Write-Host "Step 5: Merging temp_master into main (allowing unrelated histories)..." -ForegroundColor Yellow
git merge temp_master --allow-unrelated-histories

# Step 6: Push the merged result
Write-Host "Step 6: Pushing merged result to GitHub..." -ForegroundColor Yellow
git push origin main
Write-Host "✅ Pushed merged changes" -ForegroundColor Green

# Step 7: Change default branch on GitHub first
Write-Host "Merge completed successfully!" -ForegroundColor Green
Write-Host "Important: Before deleting the master branch:" -ForegroundColor Red
Write-Host "1. Go to GitHub repository Settings → Branches" -ForegroundColor Yellow
Write-Host "2. Change the default branch from 'master' to 'main'" -ForegroundColor Yellow
Write-Host "3. THEN run: git push origin --delete master" -ForegroundColor Yellow

Write-Host "Done!" -ForegroundColor Green
