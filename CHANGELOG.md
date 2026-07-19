# Changelog

## [2.0.0] - 07-19-26

- BREAKING: Package is deprecated and should be removed
  - Any scripts in use not merged into CommonTXL should be copied into project assets
- BREAKING: Moved GroupToggle, Translation scripts, extra triggers into CommonTXL

## [1.0.6] - 06-09-26

- Added enforceOnAccessUpdate (default true) to GroupToggle, to force state off if access is removed
- Updated defaultQuest option on GroupToggle to also apply to IOS clients

## [1.0.5] - 03-28-25

- Changed most internal access of GroupToggle to protected
- Added _UseStation method to StationTrigger

## [1.0.4] - 05-27-24

- Added access check to GroupToggle State property
- Added language detect to translation

## [1.0.3] - 03-28-24

- Added "Ignore Disabled Objects" and "Ignore Disabled Colliders" options to GroupToggle

## [1.0.2] - 02-13-24

- Added "Search Children" option to GroupToggle
- Internalized field access on GroupToggle

## [1.0.1] - 09-29-23

- Removed dependency on UdonSharp package to prepare for SDK 3.4.0
