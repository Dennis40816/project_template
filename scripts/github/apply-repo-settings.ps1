<#
.SYNOPSIS
  Applies the repository settings the template relies on, using the GitHub CLI.
  Rules live on GitHub, not in documents, so nobody (human or agent) can forget them.

.NOTES
  Rulesets and environment reviewers on private repositories need a paid GitHub plan.
  Steps that the plan does not allow are reported and skipped.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repo,
    [string]$Reviewer = ''
)

$ErrorActionPreference = 'Stop'

function Invoke-Gh([string]$Description, [string[]]$Arguments, [string]$Body = '') {
    Write-Host "==> $Description"
    if ($Body) {
        $Body | gh @Arguments --input - | Out-Null
    } else {
        gh @Arguments | Out-Null
    }
    if ($LASTEXITCODE -ne 0) { Write-Warning "$Description was not applied (plan limit or permission)." }
}

# Merge settings: squash only, PR title becomes the commit message, delete merged branches.
Invoke-Gh 'Merge settings' @('api', '--method', 'PATCH', "repos/$Repo",
    '-F', 'allow_squash_merge=true', '-F', 'allow_merge_commit=false', '-F', 'allow_rebase_merge=false',
    '-F', 'delete_branch_on_merge=true', '-f', 'squash_merge_commit_title=PR_TITLE',
    '-f', 'squash_merge_commit_message=PR_BODY')

# Release environment: publishing waits for a human.
$envBody = '{}'
if ($Reviewer) {
    $id = gh api "users/$Reviewer" --jq '.id'
    $envBody = (@{ reviewers = @(@{ type = 'User'; id = [int64]$id }) } | ConvertTo-Json -Depth 4 -Compress)
}
Invoke-Gh 'Release environment' @('api', '--method', 'PUT', "repos/$Repo/environments/release") $envBody
if ($LASTEXITCODE -ne 0 -and $Reviewer) {
    # Free plans reject reviewers on private repositories; keep the environment so release.yml still runs.
    Invoke-Gh 'Release environment (no reviewer)' @('api', '--method', 'PUT', "repos/$Repo/environments/release") '{}'
}

# main: pull requests only, linear history, required CI result.
$mainRuleset = @{
    name        = 'main'
    target      = 'branch'
    enforcement = 'active'
    conditions  = @{ ref_name = @{ include = @('~DEFAULT_BRANCH'); exclude = @() } }
    rules       = @(
        @{ type = 'deletion' },
        @{ type = 'non_fast_forward' },
        @{ type = 'required_linear_history' },
        @{ type = 'pull_request'; parameters = @{
            required_approving_review_count = 0; dismiss_stale_reviews_on_push = $false
            require_code_owner_review = $false; require_last_push_approval = $false
            required_review_thread_resolution = $false; allowed_merge_methods = @('squash') } },
        @{ type = 'required_status_checks'; parameters = @{
            strict_required_status_checks_policy = $false
            required_status_checks = @(@{ context = 'ci-result' }) } }
    )
} | ConvertTo-Json -Depth 8
Invoke-Gh 'Ruleset: main' @('api', '--method', 'POST', "repos/$Repo/rulesets") $mainRuleset

# Branch names: only the allowed prefixes can be created.
$namesRuleset = @{
    name        = 'branch-names'
    target      = 'branch'
    enforcement = 'active'
    conditions  = @{ ref_name = @{
        include = @('~ALL')
        exclude = @('refs/heads/main', 'refs/heads/feature/**', 'refs/heads/fix/**',
                    'refs/heads/release/**', 'refs/heads/dependabot/**') } }
    rules       = @(@{ type = 'creation' })
} | ConvertTo-Json -Depth 8
Invoke-Gh 'Ruleset: branch names' @('api', '--method', 'POST', "repos/$Repo/rulesets") $namesRuleset

# Tags: v* tags are created only by the release workflow (GitHub Actions app, id 15368).
$tagRuleset = @{
    name          = 'release-tags'
    target        = 'tag'
    enforcement   = 'active'
    conditions    = @{ ref_name = @{ include = @('refs/tags/v*'); exclude = @() } }
    rules         = @(@{ type = 'creation' }, @{ type = 'update' }, @{ type = 'deletion' })
    bypass_actors = @(@{ actor_id = 15368; actor_type = 'Integration'; bypass_mode = 'always' })
} | ConvertTo-Json -Depth 8
Invoke-Gh 'Ruleset: release tags' @('api', '--method', 'POST', "repos/$Repo/rulesets") $tagRuleset

# Issue labels used by the triage flow.
foreach ($label in @(
        @('needs-triage', 'ededed'), @('ready-for-agent', '0e8a16'), @('ready-for-human', '1d76db'),
        @('breaking', 'b60205'), @('skip-changelog', 'cccccc'))) {
    gh label create $label[0] --color $label[1] --repo $Repo --force | Out-Null
}
Write-Host 'Done.' -ForegroundColor Green
