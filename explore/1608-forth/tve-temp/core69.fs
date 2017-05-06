\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/any/varint.fs
include ../flib/i2c/bme280.fs
include ../flib/i2c/tsl4531.fs
include ../tlib/numprint.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
