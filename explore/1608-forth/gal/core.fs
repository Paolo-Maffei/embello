\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

PB10 constant LED

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

cornerstone <<<core>>>
hello
