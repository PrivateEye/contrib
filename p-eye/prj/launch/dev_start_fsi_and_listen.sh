#!/usr/bin/env bash
command_exists () {
    type "$1" &> /dev/null ;
}
if command_exists fsi; then
    fsi --load:privateeye.fsx --load:privateeye_start_profiling.fsx
else
    if command_exists fsharpi; then
        fsharpi --load:privateeye.fsx --load:privateeye_start_profiling.fsx
    else
        printf "We could not find fsi or fsharpi in your path.
        You can manually run the fsharp REPL with --load:fsharp/privateeye.fsx as the argument"
    fi
fi
