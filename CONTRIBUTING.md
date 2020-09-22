# Contributing

Try to follow the [git flow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow) development model.

The overall flow of Gitflow is:

1. A develop branch is created from master
2. A release branch is created from develop
3. Feature branches are created from develop
4. When a feature is complete it is merged into the develop branch
5. When the release branch is done it is merged into develop and master
6. If an issue in master is detected a hotfix branch is created from master
7. Once the hotfix is complete it is merged to both develop and master

## Versioning

This project will try its best to adhere to [semver](http://semver.org/) i.e, a codified guide to versioning software. When a new feature is developed or a bug is fixed the version will need to be bumped to signify the change. 

The semver string is built like this:

Major.Minor.Patch

A major version bump means that a massive change took place.

A minor version is a addition or change to the core software. Most development activity will be this type of version bump. Example: A new or reworked feature.

A patch version is a bug fix or application configuration change.

Documentation doesn't require a version bump.
