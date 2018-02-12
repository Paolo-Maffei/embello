forgetram

\ led-init
\ sd-mount.
\ ls

include cache.fs

0 [if]  \ test a simple flash page write
.s
FDISK-BASE 32 dump
$2000 32 dump
$2000 0 f-putpage
FDISK-BASE 32 dump
.s
[then]

1 [if]  \ test caching and flushing
f-init
f-buf hex.
f-buf 32 dump
$1000 32 dump
$1000 3 5 f-write
f-buf 32 dump
$2000 2 $1002 f-write
$10000 32 dump
f-buf 32 dump
f-flush
f-buf 32 dump
$11000 32 dump
[then]
