\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/i2c/mcp9808.fs
include ../flib/any/varint.fs
include ../tlib/numprint.fs
include ../tlib/lpsleep.fs
include ../tlib/rfloop.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
