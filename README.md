# catxcactivitylog

Print plain text and compressed XCode activity logs to stdout.

`Usage: catxcactivitylog <file>`

# gitstat

Print the current state of git repositories to stdout, with information about local changes, remotes, number of commits ahead or behind, etc.

`Usage: gitstat <dirs>`

# heydroid

Shorthand commands for ADB, and conveniences for profiling and logging Unity.

```
Usage: heydroid <command>
Commands:
  help
  interactive                              (Default)
  install [path/to/my.apk]
  uninstall [com.pkg.my]
  reinstall [path/to/my.apk] [com.pkg.my]
  list
  start [com.pkg.my/com.pkg.my.activity]
  kill [com.pkg.my]
  profile [com.pkg.my]                     (Unity3D only)
  log                                      (Unity3D only)
  screencap <path/to/file.png>
```

# patchdir

Patch/upgrade any directory (in source control), e.g. when upgrading the statically linked version of libcurl in your game repository.

`Usage: patchdir [-f] [-y] [-a] [-r {p4|git}] [-x <exclude> ...] [-m <match> ...] <source> <target>`

