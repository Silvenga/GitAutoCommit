![icon](docs/images/icon.png)

# GitAutoCommit

[![AppVeyor](https://img.shields.io/appveyor/ci/Silvenga/gitautocommit.svg?maxAge=2592000&style=flat-square)](https://ci.appveyor.com/project/Silvenga/gitautocommit)

A small program that automatically commits changes detected in valid Git repositories. Great for versioning everything from settings/configurations to tax documents using the power of Git. 

## Usage

```
GitAutoCommit.exe <directory>...
```

For example:
```
GitAutoCommit.exe "C:\git-repo\" ".\relative\git-repo" "etc"
```

## TODO

- [ ] UI for non-cli Usage
- [ ] Ability to push to remote