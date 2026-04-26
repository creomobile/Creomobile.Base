# Copilot Instructions

## Project Guidelines
- All repository content should be written in English.
- Prefer explicit package version management in .csproj files (including inter-package dependency versions) and avoid CI-time version substitution hacks.

## GitHub Actions
- For NuGet publishing in this repo: 
  - No prerelease tags.
  - Publish only to nuget.org.
  - No manual approval required.
  - Allow releases only from the master branch.