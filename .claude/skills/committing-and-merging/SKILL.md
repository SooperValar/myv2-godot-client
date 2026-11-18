---
name: committing-and-merging
description: Use when committing, writing a commit message, starting work on a feature or fix, creating a branch, or merging a branch into main in thelastgate-godot.
---

# Committing and merging

## Overview

Release Please builds the changelog and version numbers from the commits on `main`. Each commit on `main` is one squash merge whose message lists what changed **as a player would see it**. Anything a player wouldn't notice stays out of the changelog.

## Branches

- All work happens on a branch made from `main`, including small fixes and housekeeping. Never commit directly to `main`.
- Name branches `<type>/<short-kebab-name>`, for example `feat/world-viewport` or `fix/option-box-alignment`.

## Commits on a branch

Use the Conventional Commits format `type(scope): summary`: lower case, imperative, no full stop.

| Type | Use for | In the changelog |
|---|---|---|
| `feat` | Something new a player can see or do | Features |
| `fix` | Something a player noticed was wrong, now right | Bug Fixes |
| `perf` | Faster or smoother for the player | Performance |
| `refactor`, `test`, `docs`, `build`, `chore` | Internal work, tools under `tools/` and `game/Tools`, tests, plans, editor files | Hidden |

The scope is optional. Pick it from the area being changed: `net`, `display`, `world`, `hud`, `windows`, `launcher`, `planner`, `map`, `audio`, `items`.

## Message length

A commit message is normally just the header line. Add a prose body only when the work is complex enough that a header can't begin to explain it, for example a protocol quirk, an unusual workaround, or a trade-off a reviewer would otherwise question. Restating the header, listing changed files or describing the diff is not a reason to add a body.

## Squash merging into main

Merge only after the user has confirmed the piece works.

Write the message to a file in the scratchpad first, then:

```powershell
git switch main
git merge --squash feat/shop-sell-all
git commit -F <that message file>
git branch -D feat/shop-sell-all   # -D: git doesn't treat squashed branches as merged
```

The squash message follows this shape:

```
feat(windows): sell every unequipped item of one kind with a single click

feat(windows): show the total sale price before confirming
fix(items): stop the gold count flickering after a sale
```

`feat` or `fix` is decided by what the player had before the branch. If the behaviour already existed in this client and was wrong, it's `fix`. If the player couldn't do or see it before, it's `feat`. A problem that the branch created and then fixed itself never reached a player, so it gets no line.

- **Line 1** is the most important change for players.
- **Each further change** a player would notice gets its own `type(scope): summary` line at the **bottom** of the body. Release Please reads every one of these lines as a separate changelog entry. They are changelog entries, not a prose body, so the message-length rule doesn't apply to them. A branch with a single change has a one-line squash message.
- Write entries in the player's words: what looks or works different, not how the code does it. "Keep window text sharp at any window size" is right. "Draw the world in a SubViewport" is wrong.
- Internal changes (refactors, tests, tools, `.uid` files, editor settings) get no entry lines.
- If a branch changes nothing a player would notice, line 1 is a hidden type (for example `chore: add Godot script UIDs`) and there are no entry lines.
- Put `!` after the type (`feat!:`) only when the user says a change is breaking.

## Common mistakes

| Mistake | Fix |
|---|---|
| `git merge --no-ff` into main | Its "Merge …" commit isn't conventional and the branch's commits all land in the changelog. Always `--squash`. |
| Listing every branch commit in the squash message | List player-visible outcomes. Three commits fixing one bug is one `fix:` line. |
| Implementation words in entries ("SubViewport", "DrawRect", "refactor") | Rewrite as what the player sees. |
| Entry lines placed in the middle of the body | Put them at the bottom, one per line, with nothing after them. |
