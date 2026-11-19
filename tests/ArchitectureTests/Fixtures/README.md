# Rule fixtures

Two small fake modules, `Alpha` and `Beta`, and a fake host. `RuleFixtureTests` runs every rule in
`ModuleRules` against them to prove that each rule reports a planted violation and accepts the allowed
dependencies. Nothing here is real code. The interfaces exist only for their dependencies and visibility.

- `Alpha` breaks each rule once.
- `Beta` follows every rule.
