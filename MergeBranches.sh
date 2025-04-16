#!/bin/bash

# Script to merge unrelated Git branch histories
# Created to fix BiomorphicStructure repository

# Set error handling
set -e

# Output colorization
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting merge process for unrelated Git histories...${NC}"

# Step 1: Make sure we're up to date
echo -e "${YELLOW}Step 1: Updating local repository...${NC}"
git fetch origin
echo "✅ Fetched latest changes"

# Step 2: Make sure we're on main branch
echo -e "${YELLOW}Step 2: Switching to main branch...${NC}"
git checkout main
echo "✅ Now on main branch"

# Step 3: Merge master into main with special flag
echo -e "${YELLOW}Step 3: Merging master into main (allowing unrelated histories)...${NC}"
git merge master --allow-unrelated-histories

# Step 4: Push the merged result
echo -e "${YELLOW}Step 4: Pushing merged result to GitHub...${NC}"
git push origin main
echo "✅ Pushed merged changes"

# Step 5: Instructions for cleanup
echo -e "${GREEN}Merge completed successfully!${NC}"
echo -e "${YELLOW}Optional cleanup:${NC}"
echo -e "To delete the master branch, run: ${GREEN}git push origin --delete master${NC}"
echo -e "Or go to GitHub repository Settings → Branches and set 'main' as the default branch."

echo -e "${GREEN}Done!${NC}"
