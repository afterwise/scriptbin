# catxcactivitylog

Print plain text and compressed XCode activity logs to stdout.

`Usage: catxcactivitylog <file>`

# gitstat

Print the current state of git repositories to stdout, with information about local changes, remotes, number of commits ahead or behind, etc.

`Usage: gitstat <dirs>`

# heydroid

Shorthand commands for ADB, and conveniences for profiling and logging Unity. Start without arguments for the interactive command prompt.

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
  screenrec <path/to/file.mp4> [length]
```

Example .heydroid_config file, which means arguments can be dropped to some commands:
```
HEYDROID_SDK=/usr/local/android
HEYDROID_APK=my.apk
HEYDROID_PKG=com.pkg.my
HEYDROID_APP=com.pkg.my.activity
```

# patchdir

Patch/upgrade any directory (in source control), e.g. when upgrading the statically linked version of libcurl in your game repository.

`Usage: patchdir [-f] [-y] [-a] [-r {p4|git}] [-x <exclude> ...] [-m <match> ...] <source> <target>`

# svnmove

A move, rename and renumber tool for svn.

`Usage: svnmove [-y][-n] <new-path> <old-path ...>`

Examples:
```
$ svnmove /c /c/foo/quux*.wav
/c/foo/quux_01.wav -> /c/quux_01.wav
/c/foo/quux_03.wav -> /c/quux_03.wav
/c/foo/quux_05.wav -> /c/quux_05.wav
/c/foo/quux_07.wav -> /c/quux_07.wav
/c/foo/quux_09.wav -> /c/quux_09.wav
```
```
$ svnmove /c/bar /c/foo/quux*.wav
/c/foo/quux_01.wav -> /c/bar_01.wav
/c/foo/quux_03.wav -> /c/bar_03.wav
/c/foo/quux_05.wav -> /c/bar_05.wav
/c/foo/quux_07.wav -> /c/bar_07.wav
/c/foo/quux_09.wav -> /c/bar_09.wav
```
```
$ svnmove -n /c/bar /c/foo/quux*.wav
/c/foo/quux_01.wav -> /c/bar_01.wav
/c/foo/quux_03.wav -> /c/bar_02.wav
/c/foo/quux_05.wav -> /c/bar_03.wav
/c/foo/quux_07.wav -> /c/bar_04.wav
/c/foo/quux_09.wav -> /c/bar_05.wav
```

# svnstash

A dead-simple stash for svn, similar to git-stash.

```
svnstash <command> [match]

commands:
        help         - show this help
        update       - update stash list with modified files in the repository
        status       - show current stash list
        discard      - remove file from stash
        obliterate   - remove entire stash
        stow         - copy file into stash and revert in the repository
        reset        - revert a stashed file in the repository
        restore      - copy a stashed file back into the repository
        diff         - show diff between stashed files and repository
        changelist   - add or remove repository files from the stash list from a changelist
```

