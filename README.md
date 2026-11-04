# D3sk.AgentCli

The .NET port of [AgentCliBridge](https://github.com/neijix/AgentCliBridge): runs the AI coding
CLIs (Claude Code, Cursor, Gemini, Copilot, Grok, OpenCode) and folds their output streams into
typed events. Package `D3sk.AgentCli`, used by D3SKHAND.

## Frozen fixtures

`tests/streams` and `tests/wire` are byte-for-byte copies of the Python repo at tag `v0.3.0`
(commit `e493c737baee2e11621ea80199ef332f48c3b7f0`). `tests/frozen-manifest.txt` records the tag
and each file's SHA-256; `FrozenFixturesTests` fails when a file differs, is missing or is extra.
Never edit those files: a new capture goes in a new folder with its own manifest entry.

## Build and test

Needs the .NET 10 SDK (`global.json`).

```bash
dotnet test D3sk.AgentCli.slnx
dotnet pack src/D3sk.AgentCli -c Release
```

Restoring this repo needs no credential: it has no `D3sk.*` dependency yet.

## Install the package

The package is published to GitHub Packages, which needs a token even for a public package: a
classic personal access token with `read:packages`. Copy this repo's `nuget.config` into the
consuming repo, then set the credential in the environment, never in a file:

```bash
export NuGetPackageSourceCredentials_github="Username=<github-login>;Password=<token>"
```

In GitHub Actions use the job's `GITHUB_TOKEN` with `permissions: packages: read`.

## Release

Tag `vX.Y.Z`; `.github/workflows/publish.yml` tests, packs with that version and pushes to
`https://nuget.pkg.github.com/neijix/index.json` using the job's `GITHUB_TOKEN`. No stored secret.

## Licence

MIT, see `LICENSE`.
