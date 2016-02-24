#!/bin/bash

if (! $TRAVIS) then
    pushd "$(dirname "$0")"
fi

rm -rf artifacts
if ! type dnvm > /dev/null 2>&1; then
    curl -sSL https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.sh | DNX_BRANCH=dev sh && source ~/.dnx/dnvm/dnvm.sh
fi

# work around restore timeouts on Mono
[ -z "$MONO_THREADS_PER_CPU" ] && export MONO_THREADS_PER_CPU=50

export DNX_UNSTABLE_FEED=https://www.myget.org/F/aspnetcidev/api/v2
dnvm update-self

dnvm install 1.0.0-rc2-16444 -u -r mono
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

dnvm install 1.0.0-rc2-16444 -u -r coreclr
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

dnu restore --quiet --parallel
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
