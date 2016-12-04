\ core definitions

\ <<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/spi/smem.fs
include ../flib/i2c/oled.fs

include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../tlib/oled.fs
include ../tlib/numprint.fs
include ../flib/any/varint.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
