# Conventional commits

CryptoGuide uses [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/).
This document is the shared commit policy for backend, frontend and documentation changes.

## Message format

```text
<type>[optional scope][optional !]: <ticket ID> <short description>

[optional body explaining why]

[optional footers]
```

Use lowercase types, a short imperative description ("add", "fix", "document"), and no trailing full stop. Include the relevant ticket ID after the colon. The ticket ID is our project convention, not a requirement of the specification. For maintenance without a ticket, omit the ID rather than inventing one.

| Type | Use |
|---|---|
| `feat` | Add application functionality |
| `fix` | Correct a bug |
| `docs` | Change documentation only |
| `refactor` | Restructure code without changing behavior |
| `test` | Add or change tests |
| `build` | Change dependencies or build configuration |
| `ci` | Change automation pipelines |
| `style` | Change formatting without changing behavior |
| `perf` | Improve performance |
| `chore` | Repository setup or other maintenance |

Optional scopes describe an area, such as `api`, `frontend`, `deps`, or `repo`. The specification defines `feat` and `fix`; the additional types above are our chosen vocabulary.

## Examples

```text
chore(repo): CG-001 initialize repository and backend skeleton
feat(api): CG-001 add JSON health endpoint
docs: CG-001 document local setup
build(deps): CG-001 update OpenAPI dependency
```

For the initial commit containing the repository scaffold and minimal backend, use:

```text
chore(repo): CG-001 initialize CryptoGuide repository and health endpoint
```

Use that message once the described work is verified. One coherent initial commit is appropriate; subsequent commits should group related changes and explain their purpose.

Mark an incompatible API change with `!` immediately before the colon or a `BREAKING CHANGE:` footer. Explain the impact and migration when applicable:

```text
feat(api)!: CG-007 change market response envelope

BREAKING CHANGE: Market clients must read coin entries from the items property.
```

## Before committing

1. Build and manually verify the behavior relevant to the ticket.
2. Review staged changes with `git diff --cached` and `git status --short`.
3. Ensure credentials, local configuration, `bin/`, `obj/`, and `node_modules/` are excluded.
4. Update setup instructions or acceptance evidence when needed.

Keep the branch convention from the backlog, for example `feature/CG-005-cmc-client`.
Hooks and commit-message tooling are optional future improvements, outside CG-001.
